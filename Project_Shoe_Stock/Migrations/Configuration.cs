namespace Project_Shoe_Stock.Migrations
{
    using Project_Shoe_Stock.Models;
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<Project_Shoe_Stock.Models.ShoeDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(Project_Shoe_Stock.Models.ShoeDbContext context)
        {
            context.Brands.AddRange(new Brand[] {
                new Brand { BrandName = "Puma" },
                new Brand { BrandName = "Nike" }
            });
            context.ShoeModels.AddRange(new ShoeModel[]
            {
                new ShoeModel{ ModelName="Air"},
                new ShoeModel{ModelName="Steps"}
            });
            context.SaveChanges();
            Shoe s = new Shoe
            {
                Name = "Temu",
                ShoeModelId = 1,
                BrandId = 1,
                FirstIntroducedOn = new DateTime(2024, 1, 1),
                OnSale = true,
                Picture = "1.jpg"
            };
            s.Stocks.Add(new Stock { Size = Size.A40, Quantity = 10, Price = 900 });
            s.Stocks.Add(new Stock { Size = Size.A43, Quantity = 10, Price = 950 });
            context.Shoes.Add(s);
            context.SaveChanges();
        }
    }
}
