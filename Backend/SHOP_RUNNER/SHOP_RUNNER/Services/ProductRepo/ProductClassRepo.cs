using Azure.Core;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Asn1.Ocsp;
using SHOP_RUNNER.Common;
using SHOP_RUNNER.DTOs.Brand_DTO;
using SHOP_RUNNER.DTOs.Category_DTO;
using SHOP_RUNNER.DTOs.Color_DTO;
using SHOP_RUNNER.DTOs.Gender_DTO;
using SHOP_RUNNER.DTOs.Product_DTO;
using SHOP_RUNNER.DTOs.Size_DTO;
using SHOP_RUNNER.Entities;
using SHOP_RUNNER.Models.Product_Model;
using System.Collections.Generic;
using System.Globalization;

namespace SHOP_RUNNER.Services.ProductRepo
{
    public class ProductClassRepo : IProductRepo
    {
        private readonly RunningShopContext _context;
        public static int PAGE_SIZE { get ; set; } = 3;
        public ProductClassRepo(RunningShopContext context) { 
            _context = context;
        }


        /*
         
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot\Uploads\Images");
        if (!Directory.Exists(filePath))
        {
             Directory.CreateDirectory(filePath);
        }
         
         */

        // Xong add
        public ProductGetAll AddProduct(CreateProduct product, string url )
        {
            #region HANDLE THUMBNAIL
            /*             string path = "wwwroot\\Uploads\\Images";
            string filename = Guid.NewGuid().ToString() + Path.GetExtension( product.Thumbnail.FileName );

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), path);
            if (!Directory.Exists(filePath))
            {
                Directory.CreateDirectory(filePath);
            }

            // var upload = Path.Combine(  Directory.GetCurrentDirectory(), path, filename );
            var upload = Path.Combine(filePath, filename);

            product.Thumbnail.CopyTo(new FileStream(upload, FileMode.Create));

            // using ( var fileStream = new FileStream(upload, FileMode.Create))
            //{ product.thumbnail.CopyTo(fileStream); }

            string url = $"{Request.Scheme}://{Request.Host}/uploads/{filename}";*/
            #endregion

            Product new_Product = new Product()
            {
                Name = product.Name,
                Price = product.price,
                Description = product.description,
                Thumbnail = url, // neu ko co anh la 
                Qty = product.qty,
                CategoryId = product.category_id,
                CreateDate = DateTime.Now,
                UserId = product.user_id,
                GenderId = product.gender_id,
                BrandId = product.brand_id,
                SizeId = product.size_id,
                ColorId = product.color_id,
            };

            _context.Products.Add(new_Product);
            _context.SaveChanges();

           Product p = _context.Products.Where(p => p.Name == product.Name).Include(p=>p.Category).First();

            return new ProductGetAll
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                Description = p.Description,
                Thumbnail = p.Thumbnail,
                Qty = p.Qty,
                CategoryId = p.CategoryId,
                Category = new CategoryGetAll()
                {
                    Id = p.Category.Id,
                    Name = p.Category.Name,
                }
            };
        }


        // xong xoá
        public void DeleteProduct(int id)
        {
            var product = _context.Products.FirstOrDefault(p => p.Id == id);

            if (product != null)
            {
                _context.Products.Remove(product);
                _context.SaveChanges();
            }
        }




        // Xong getAll:
        public List<ProductGetAll> GetAll()
        {
            
            var products = _context.Products.Include( p => p.Category ).ToList();

            List<ProductGetAll> ListDTO = new List<ProductGetAll>();

            foreach (var product in products)
            {
                if (product.IsValid == true) {
                    ListDTO.Add(new ProductGetAll()
                    {
                        Id = product.Id,
                        Name = product.Name,
                        Price = product.Price,
                        Description = product.Description,
                        Thumbnail = product.Thumbnail,
                        Qty = product.Qty,
                        CategoryId = product.CategoryId,
                        Category = new CategoryGetAll()
                        {
                            Id = product.Category.Id,
                            Name = product.Category.Name,
                        }
                    });
                }
               
            }

            return ListDTO;
        }



        // xong get detail
        public ProductDetail GetDetail(int id)
        {
           Product p = _context.Products
                .Where(p => p.Id == id)
                .Include(p => p.Category)
                .Include(p => p.Gender)
                .Include(p=> p.Brand)
                .Include(p=> p.Size)
                .Include(p=> p.Color)
                .First();

            if (p == null)
            {
                return null;
            }

            if (p.IsValid == false)
            {
                return null;
            }

            return new ProductDetail()
            {
                Id = id,
                Name = p.Name,
                Price = p.Price,
                Description = p.Description,
                Thumbnail = p.Thumbnail,
                Qty = p.Qty,
                CategoryId = p.CategoryId,
                GenderId = p.GenderId,
                BrandId = p.BrandId,
                SizeId = p.SizeId,
                ColorId = p.ColorId,
                CreateDate = p.CreateDate,
                Category = new CategoryGetAll()
                {
                    Id = p.Category.Id,
                    Name = p.Category.Name,
                },
                Gender = new GenderGetAll()
                {
                    Id = p.Gender.Id,
                    Name = p.Gender.Name,
                },
                Brand = new BrandGetAll()
                {
                    Id = p.Brand.Id,
                    Name = p.Brand.Name
                },
                Size = new SizeGetAll()
                {
                    Id = p.Size.Id,
                    Name = p.Size.Name
                },
                Color = new ColorGetAll()
                {
                    Id = p.Color.Id,
                    Name = p.Color.Name
                }
            };











        }


        // xong related
        public List<ProductGetAll> Relateds(int id)
        {
            var p = _context.Products.Find(id);

            if (p == null)
            {
                return null;
            }

            var products = _context.Products.Where(p => p.Id != id).Where(p => p.CategoryId == p.CategoryId).Include(p => p.Category).Take(4).OrderByDescending(p => p.Id).ToList();

            List<ProductGetAll> list_p = new List<ProductGetAll>();

            foreach (var product in products)
            {
                if (product.IsValid == true)
                {
                    list_p.Add(new ProductGetAll()
                    {
                        Id = product.Id,
                        Name = product.Name,
                        Price = product.Price,
                        Description = product.Description,
                        Thumbnail = product.Thumbnail,
                        Qty = product.Qty,
                        CategoryId = product.CategoryId,
                        Category = new CategoryGetAll()
                        {
                            Id = product.Category.Id,
                            Name = product.Category.Name,
                        }
                    });
                }

               
            }
            return list_p;

        }


        // ko search đc description
        // xong search
        public List<ProductGetAll> Search(string search, int page)
        {


            //  var allProducts = _context.Products
            //          .Where(p => (p.Name.Contains(search) || EF.Functions.Like(EF.Property<string>(p, "Description"), $"%{search}%")) )
            //         .Include(p => p.Category)
            //         .ToList();



            var allProducts = _context.Products.AsQueryable().Where(p => (p.Name.Contains(search) || EF.Functions.Like(EF.Property<string>(p, "Description"), $"%{search}%"))).Include(p => p.Category);

            // var result = PaginationList<Product>.Create(Products, page, pagesize > 3 ? pagesize : PAGE_SIZE);

            var results = PaginationList<Product>.Create(allProducts, page, 10);

            List<ProductGetAll> list_p = new List<ProductGetAll>();

            foreach ( var product in results)
            {
                if (product.IsValid == true)
                {
                    list_p.Add(new ProductGetAll()
                    {
                        Id = product.Id,
                        Name = product.Name,
                        Price = product.Price,
                        Description = product.Description,
                        Thumbnail = product.Thumbnail,
                        Qty = product.Qty,
                        CategoryId = product.CategoryId,
                        Category = new CategoryGetAll()
                        {
                            Id = product.Category.Id,
                            Name = product.Category.Name,
                        }
                    });
                }

            }
            return list_p;
        }


       
        public ProductGetAll UpdateProduct(EditProduct product, string url)
        {
            var product_new = _context.Products.Where(p => p.Id == product.Id).Include(p => p.Category).First();
            /*  
            


            // handle file:
            string path = "wwwroot\\Uploads\\Images";
            string filename = Guid.NewGuid().ToString() + Path.GetExtension(product.Thumbnail.FileName);
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), path);
            if (!Directory.Exists(filePath))
            {
                Directory.CreateDirectory(filePath);
            }
            var upload = Path.Combine(filePath, filename);

            product.Thumbnail.CopyTo(new FileStream(upload, FileMode.Create));
            string url = $"/Uploads/Images/{filename}";

            */



            if (product_new != null)
            {
                product_new.Name = product.Name;
                product_new.Price = product.price;
                product_new.Description = product.description != ""?product.description : product_new.Description;
                product_new.Thumbnail = product.Thumbnail!= null? url : product_new.Thumbnail;
                product_new.Qty = product.qty;
                product_new.CategoryId = product.categoryId;
                product_new.GenderId = product.genderId;
                product_new.BrandId = product.brandId;
                product_new.SizeId = product.sizeId;
                product_new.ColorId = product.colorId;
                _context.SaveChanges();

                return new ProductGetAll()
                {
                    Id = product.Id,
                    Name = product.Name,
                    Price = product.price,
                    Description = product.description,
                    Thumbnail = product.Thumbnail != null ? url : product_new.Thumbnail,
                    Qty = product.qty,
                    CategoryId = product.categoryId,
                    Category = new CategoryGetAll()
                    {
                        Id = product_new.Category.Id,
                        Name = product_new.Category.Name,
                    }
                };
                   
            }

            return null;


        }

       public Object Paging(int page, int pagesize)
        {
            var Products = _context.Products.AsQueryable().Include(p => p.Category);

            var result = PaginationList<Product>.Create(Products, page, pagesize > 3 ? pagesize : PAGE_SIZE);

            List<ProductGetAll> New_List = new List<ProductGetAll>();


            foreach (var product in result) {

                if (product.IsValid == true)
                {

                    New_List.Add(new ProductGetAll()
                    {
                        Id = product.Id,
                        Name = product.Name,
                        Price = product.Price,
                        Description = product.Description,
                        Thumbnail = product.Thumbnail,
                        Qty = product.Qty,
                        CategoryId = product.CategoryId,
                        Category = new CategoryGetAll()
                        {
                            Id = product.Category.Id,
                            Name = product.Category.Name
                        }
                    });
                }
            }

            return new
            {
                New_List = New_List,
                Count_P = Products.Count()
            };
        }



       
       public List<ProductGetAll> Filter(double? from, double? to, string? category, string? gender, string? brand, string? size, string? color, int page)
        {
            // THỰC HIỆN TẠO MỘT TRUY VẤN CÓ THỂ HOLD ĐỢI CÁC THUỘC TÍNH KHÁC  (bằng cách sd hàm .AsQueryable() ) LẤY VỀ ALL DATA
            var Products = _context.Products.AsQueryable();

            #region FILTER_PRICE
            if (from.HasValue)
            {
                Products = Products.Where(p => p.Price >= Convert.ToDecimal(from));
            }

            if (to.HasValue)
            {
                Products = Products.Where(p => p.Price <= Convert.ToDecimal(to));
            }
            #endregion

            Products = Products.Include(p => p.Category).Include(p => p.Gender).Include(p => p.Brand).Include(p => p.Size).Include(p => p.Color);

            #region FILTER CATE GENDER BRAND SIZE COLOR

            if (!string.IsNullOrEmpty(category))
            {
                //Products = Products.Where(p => p.Category.Name == category);
                Products = Products.Where(p => EF.Functions.Like(p.Category.Name, category));
            }

            if (!string.IsNullOrEmpty(gender))
            {
                // Products = Products.Where(p => p.Gender.Name == gender);
                Products = Products.Where(p => EF.Functions.Like(p.Gender.Name, gender));
            }

            if (!string.IsNullOrEmpty(brand))
            {
                // Products = Products.Where(p => p.Brand.Name == brand);
                Products = Products.Where(p => EF.Functions.Like(p.Brand.Name, brand));
            }

            if (!string.IsNullOrEmpty(size))
            {
                //Products = Products.Where(p => p.Size.Name == size);
                Products = Products.Where(p => EF.Functions.Like(p.Size.Name, size));
            }

            if (!string.IsNullOrEmpty(color))
            {
                //Products = Products.Where(p => p.Color.Name == color);
                Products = Products.Where(p => EF.Functions.Like( p.Color.Name, color)); 
            }

            #endregion

            // if it does not have any case -> sort all products and return for client:
            Products = Products.OrderBy(p => p.Name);

            var results = PaginationList<Product>.Create(Products, page, 10);

            // Products.ToList();


            List<ProductGetAll> List_p = new List<ProductGetAll>();

            foreach (var p in results)
            {
                if (p.IsValid == true)
                {
                    List_p.Add(new ProductGetAll()
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Price = p.Price,
                        Description = p.Description,
                        Thumbnail = p.Thumbnail,
                        Qty = p.Qty,
                        CategoryId = p.CategoryId,
                        Category = new CategoryGetAll()
                        {
                            Id = p.Category.Id,
                            Name = p.Category.Name
                        }

                    });
                }
            }

            return List_p;

        }


        public List<ProductGetAll> Sort(string? sortBy, int page)
        {
            var Products = _context.Products.AsQueryable();

            Products = Products.OrderBy(p => p.Name);

            if (!string.IsNullOrEmpty(sortBy))
            {
                switch (sortBy)
                {
                    case "NAME_DESC": Products = Products.OrderByDescending(p => p.Name); break;
                    case "PRICE_ASC": Products = Products.OrderBy(p => p.Price); break;
                    case "PRICE_DESC": Products = Products.OrderByDescending(p => p.Price); break;
                }
            }

            Products = Products.Include(p => p.Category);

            var results = PaginationList<Product>.Create(Products, page, 10);


            List<ProductGetAll> list_p = new List<ProductGetAll>();

            foreach (var p in results)
            {
                if (p.IsValid == true)
                {
                    list_p.Add(new ProductGetAll()
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Price = p.Price,
                        Description = p.Description,
                        Thumbnail = p.Thumbnail,
                        Qty = p.Qty,
                        CategoryId = p.CategoryId,
                        Category = new CategoryGetAll()
                        {
                            Id = p.Category.Id,
                            Name = p.Category.Name
                        }
                    });
                }

            }
            return list_p;
        }



        public void turn_off_p(int p_id)
        {
            var product = _context.Products.FirstOrDefault( p => p.Id == p_id );

            if ( product == null )
            {
                return;
            }

            if (product.IsValid == true)
            {
                product.IsValid = false;
            }
            else
            {
                product.IsValid = true;
            }
           
            _context.SaveChanges();
        }






    }
}
