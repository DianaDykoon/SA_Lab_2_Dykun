using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleBot
{
    public class Stock
    {
        static int currentNumber = 1;
        [Key]
        private int id;
        private string? name;
        private string? description;
        private float oldPrice;
        private float newPrice;
        private string? store;
        private string? productUrl;
        private string? photoUrl;

        public int Id { get => id; set => id = value; }
        public string Name
        {
            get
            {
                if (!string.IsNullOrEmpty(name))
                    return name;
                return "Назва";
            }
            set => name = value;
        }
        public string Description
        {
            get
            {
                if (!string.IsNullOrEmpty(description))
                    return description;
                return "Опис";
            }
            set => description = value;
        }
        public float OldPrice { get => oldPrice; set => oldPrice = value; }
        public float NewPrice { get => newPrice; set => newPrice = value; }
        public string Store
        {
            get
            {
                if (!string.IsNullOrEmpty(store))
                    return store;
                return "Магазин";
            }
            set => store = value;
        }

        public string ProductUrl
        {
            get
            {
                if (!string.IsNullOrEmpty (productUrl))
                    return productUrl;
                return "---";
            }
            set => productUrl = value;
        }

        public string PhotoUrl
        {
            get
            {
                if (!string.IsNullOrEmpty(photoUrl))
                    return photoUrl;
                return "https://github.com/TelegramBots/book/raw/master/src/docs/photo-ara.jpg";
            }
            set => photoUrl = value;
        }

        public Stock (string name, string description, float oldPrice,
            float newPrice, string store, string productUrl, string photoUrl)
        {
            Name = name;
            Description = description;
            OldPrice = oldPrice;
            NewPrice = newPrice;
            Store = store;
            ProductUrl = productUrl;
            PhotoUrl = photoUrl;

            Id = currentNumber++;
        }

        public double Sale
        {
            get { return 100 - (NewPrice * 100/OldPrice); }
        }

        public override string ToString()
        {
            return $"<b>Знижка {Sale:F1}%</b>\n" +
                $"{Name}\n\n" +
                $"Опис: \n{Description}" +
                $"\n----------------" +
                $"\n<strike>{OldPrice:F2} ₴</strike>" +
                $"\n{NewPrice:F2} ₴" +
                $"\nКупуй в магазині <b>{Store}</b>\n" +
                $"{ProductUrl}";
        }
    }
}
