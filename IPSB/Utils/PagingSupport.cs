using AutoMapper;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices;

namespace IPSB.Utils
{
    public interface IPagingSupport<T>
    {
        ///<summary>
        /// Get the total entity count.
        ///</summary>
        int Count { get; }

        ///<summary>
        /// Add query source.
        ///</summary>
        PagingSupport<T> From(IQueryable<T> source);

        ///<summary>
        /// Get a range of persited entities.
        ///</summary>
        PagingSupport<T> GetRange(int pageIndex, int pageSize, Expression<Func<T, object>> orderBy, bool isAll, bool sortOrder, bool random = false);



        ///<summary>
        /// Get paginated result.
        ///</summary>
        Paged<TResult> Paginate<TResult>([Optional] Func<TResult, T, TResult> transform);
    }

    public class PagingSupport<T> : IPagingSupport<T>
    {
        private readonly IMapper _mapper;
        private IQueryable<T> _source;
        private IQueryable<T> _sourcePageSize;
        private int _pageIndex;
        private int _pageSize;

        public PagingSupport(IMapper mapper)
        {
            _mapper = mapper;
        }

        public PagingSupport<T> From(IQueryable<T> source)
        {
            this._source = source;
            return this;
        }

        public int Count
        {
            get { return _source.Count(); }
        }

        public PagingSupport<T> GetRange(int pageIndex, int pageSize, Expression<Func<T, object>> orderBy, bool isAll, bool sortOrder = true, bool random = false)
        {
            // If random and isAll is both specified at the same time
            if (random && isAll)
            {
                throw new ArgumentException("[random = true] and [isAll = true] is specifed at the same time!");
            }

            _pageIndex = pageIndex;
            _pageSize = pageSize;
            int toSkip = (pageIndex - 1) * pageSize;

            if (random)
            {
                int totalCount = _source.Count();
                int newToSkip = new Random().Next(1, totalCount);
                if (totalCount - newToSkip >= pageSize)
                {
                    toSkip = newToSkip;
                }
                _sourcePageSize = _source.OrderBy(x => Guid.NewGuid());
            }
            else
            {
                if (sortOrder)
                {
                    _sourcePageSize = _source.OrderBy(orderBy);
                }
                else
                {
                    _sourcePageSize = _source.OrderByDescending(orderBy);
                }
            }

            if (!isAll)
            {
                _sourcePageSize = (_sourcePageSize ?? _source).Skip(toSkip).Take(pageSize);
            }
            return this;
        }

        public Paged<TResult> Paginate<TResult>([Optional] Func<TResult, T, TResult> transform)
        {
            int count = Count;

            var pagingVM = new Paged<TResult>()
            {
                TotalCount = Count,
                PageSize = _pageSize,
                TotalPage = (int)Math.Ceiling((double)Count / _pageSize),
                CurrentPage = _pageIndex
            };
            if (transform != null)
            {
                pagingVM.Content = _sourcePageSize?.Select(_ => transform(_mapper.Map<TResult>(_), _));
            }
            else
            {
                pagingVM.Content = _sourcePageSize?.Select(_ => _mapper.Map<TResult>(_));
            }

            if (_pageIndex > 1)
            {
                pagingVM.PreviousPage = _pageIndex - 1;
            }

            if (_pageIndex < count && _pageIndex + 1 <= pagingVM.TotalPage)
            {
                pagingVM.NextPage = _pageIndex + 1;
            }
            else
            {
                pagingVM.NextPage = _pageIndex;
            }

            return pagingVM;
        }

    }

    public class Paged<T>
    {
        public int TotalCount { get; set; }
        public int PageSize { get; set; }
        public int TotalPage { get; set; }
        public int CurrentPage { get; set; }
        public int? NextPage { get; set; }
        public int? PreviousPage { get; set; }
        public IQueryable<T> Content { get; set; }
    }
}
