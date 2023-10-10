namespace SHOP_RUNNER.Common
{
    public class PaginationList<T> : List<T>
    {
        public int PageIndex { get; set; }
        public int totalPage { get; set; }


        public PaginationList( List<T> items, int count, int PageIndex, int pageSize ) {
            PageIndex = PageIndex;
            
            totalPage = (int)Math.Ceiling( count / (double) pageSize ); // toltal elements divide for elements in a page -> caculate result to double -> cast into int 

            AddRange(items);
            // AddRange(items) thêm nhiều phần tử vào danh sách một lần, thay vì phải dùng vòng lặp và gọi phương thức Add từng phần tử
        }


        public static PaginationList<T> Create(IQueryable<T> source, int pageIndex, int pageSize)
        {
            var count = source.Count(); // source's length
            var items = source.Skip( (pageIndex - 1)*pageSize ).Take(pageSize).ToList(); // page number index present - 1 * page's elements


            return new PaginationList<T>(items, count, pageIndex, pageSize);
        }

    }
}


/*
 
Đúng rồi, bạn đã hiểu đúng. Class PaginationList<T> kế thừa từ List<T>, 
nghĩa là nó sẽ có tất cả các tính năng của một List<T> và có thể chứa các phần tử kiểu T. 
Bạn cũng có thể thêm các thuộc tính và phương thức mới vào PaginationList<T> 

 */