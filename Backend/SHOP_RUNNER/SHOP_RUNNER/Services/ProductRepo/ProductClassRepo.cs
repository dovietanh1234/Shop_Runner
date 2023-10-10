using Microsoft.EntityFrameworkCore;
using SHOP_RUNNER.DTOs.Brand_DTO;
using SHOP_RUNNER.DTOs.Category_DTO;
using SHOP_RUNNER.DTOs.Color_DTO;
using SHOP_RUNNER.DTOs.Gender_DTO;
using SHOP_RUNNER.DTOs.Product_DTO;
using SHOP_RUNNER.DTOs.Size_DTO;
using SHOP_RUNNER.Entities;
using SHOP_RUNNER.Models.Product_Model;
using System.Collections.Generic;

namespace SHOP_RUNNER.Services.ProductRepo
{
    public class ProductClassRepo : IProductRepo
    {
        private readonly RunningShopContext _context;
        public ProductClassRepo(RunningShopContext context) { 
            _context = context;
        }

        // Xong add
        public ProductGetAll AddProduct(CreateProduct product)
        {
            Product new_Product = new Product()
            {
                Name = product.Name,
                Price = product.price,
                Description = product.description,
                Thumbnail = product.thumbnail,
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
            return list_p;

        }


        // ko search đc description
        // xong search
        public List<ProductGetAll> Search(string search)
        {
            var allProducts = _context.Products
                .Where(p => (p.Name.Contains(search) || EF.Functions.Like(EF.Property<string>(p, "Description"), $"%{search}%")) )
                .Include(p => p.Category)
                .ToList();

            List<ProductGetAll> list_p = new List<ProductGetAll>();

            foreach ( var product in allProducts)
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
            return list_p;
        }


        // sửa đang sai
        public void UpdateProduct(EditProduct product)
        {
            var product_new = _context.Products.FirstOrDefault(p => p.Id == product.Id);

            if(product_new != null)
            {
                product_new.Name = product.Name;
                product_new.Price = product.price;
                product_new.Description = product.description != ""?product.description : product_new.Description;
                product_new.Thumbnail = product.thumbnail!=""?product.thumbnail : product_new.Thumbnail;
                product_new.Qty = product.qty;
                product_new.CategoryId = product.category_id;
                product_new.GenderId = product.gender_id;
                product_new.BrandId = product.brand_id;
                product_new.SizeId = product.size_id;
                product_new.ColorId = product.color_id;
                _context.SaveChanges();
            }
        }



    }
}
