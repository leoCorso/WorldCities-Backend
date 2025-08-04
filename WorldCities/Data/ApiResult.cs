using System.Reflection;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;

namespace WorldCities.Data
{
    public class ApiResult<T>
    {
        private ApiResult(
            List<T> data,
            int count,
            int pageIndex,
            int pageSize,
            string? sortColumn,
            string? sortOrder,
            string? filterColumn,
            string? filterQuery
            )
        {
            Data = data;
            TotalCount = count;
            PageIndex = pageIndex;
            PageSize = pageSize;
            TotalPages = (int)Math.Ceiling((double)TotalCount / PageSize);
            SortColumn = sortColumn;
            SortOrder = sortOrder;
            FilterColumn = filterColumn;
            FilterQuery = filterQuery;
        }

        public static async Task<ApiResult<T>> CreateAsync(
            IQueryable<T> source,
            int pageIndex,
            int pageSize,
            string? sortColumn = null,
            string? sortOrder = null,
            string? filterColumn = null,
            string? filterQuery = null
            )
        {
            if (!string.IsNullOrEmpty(sortColumn) && IsValidProperty(sortColumn))
            {
                sortOrder = !string.IsNullOrEmpty(sortOrder) && sortOrder.ToUpper() == "ASC" ? "ASC" : "DESC";
                source = source.OrderBy(
                    string.Format("{0} {1}", sortColumn, sortOrder)
                    );
            }
            if (!string.IsNullOrEmpty(filterColumn) && IsValidProperty(filterColumn) && !string.IsNullOrEmpty(filterQuery))
            {
                source = source.Where(string.Format("{0}.startsWith(@0)", filterColumn), filterQuery);
            }
            var count = await source.CountAsync();
            source = source.Skip(pageIndex * pageSize).Take(pageSize);

            var data = source.ToList();
            return new ApiResult<T>(
                data,
                count,
                pageIndex,
                pageSize,
                sortColumn,
                sortOrder,
                filterColumn,
                filterQuery
            );
        }
        public static bool IsValidProperty(string propertyName, bool throwExceptionIfNotFound = true)
        {
            var prop = typeof(T).GetProperty(
                propertyName,
                BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance
                );
            if(prop == null && throwExceptionIfNotFound)
            {
                throw new NotSupportedException($"ERROR: Property '{propertyName}' does not exist.");
            }
            return prop != null;
        }
        public List<T> Data { get; private set; }
        public int PageIndex { get; private set; }
        public int PageSize { get; private set; }
        public int TotalCount { get; private set; }

        public int TotalPages { get; private set; }
        public bool HasNextPage
        {
            get
            {
                return ((PageIndex + 1) < TotalPages);
            }
        }
        public bool HasPreviousPage
        {
            get
            {
                return (PageIndex > 0);
            }
        }
        public string? SortColumn { get; private set; }
        public string? SortOrder { get; private set; }

        public string? FilterColumn { get; private set; }
        public string? FilterQuery { get; private set; }
    }
}
