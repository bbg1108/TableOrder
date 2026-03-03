using Kiosk.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Kiosk.CommonEnum;

namespace Kiosk
{
    public class DataManager
    {
        private static DataManager _instance;
        public static DataManager instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new DataManager();
                }
                return _instance;
            }
        }

        private readonly List<Product> _AllProducts;                     // 모든 메뉴의 물품 정보
        private readonly List<OrderItem> _SelectedOrderItems;            // 선택한 물품 리스트

        public int TotalPrice { get; private set; }                     // 선택한 물품의 총 가격
        public KioskScreenEnum PreviousScreen { get; private set; }     // 이전 키오스크 화면
        public KioskScreenEnum CurrentScreen { get; private set; }      // 현재 키오스크 화면
        public bool IsVoiceMode { get; set; }                           // 음성인식 모드인지 상태
        public bool IsTTSSpeaking { get; set; }                         // TTS가 말하고 있는지 상태
        public int TableNo { get; set; }                                // 테이블 번호

        private DataManager()
        {
            _AllProducts = new List<Product>();
            _SelectedOrderItems = new List<OrderItem>();
        }

        #region AllProducts 관련 함수
        public void CopyAllProducts(List<Product> products)
        {
            _AllProducts.AddRange(products);
        }

        public IReadOnlyList<Product> GetAllProducts()
        {
            IReadOnlyList<Product> list = _AllProducts;
            return list;
        }

        public Product GetOneProduct(string name)
        {
            return _AllProducts.FirstOrDefault(x => x.Name == name);
        }

        public void ClearAllProduct()
        {
            _AllProducts.Clear();
        }

        public string GetProductImagePath(string name)
        {
            string path = GetOneProduct(name).ImagePath;
            return path;
        }
        #endregion

        #region SelectedOrderItems 관련 함수
        public List<OrderItem> GetSelectedOrderItems()
        {
            var orderItems = new List<OrderItem>(); // 깊은 복사
            foreach (var item in _SelectedOrderItems)
            {
                orderItems.Add(new OrderItem
                {
                    Name = item.Name,
                    Count = item.Count,
                    Price = item.Price,
                });
            }
            return orderItems;
        }

        public void AddSelectedOrderItem(Product item)
        {
            var matchItem = _SelectedOrderItems.FirstOrDefault(x => x.Name == item.Name);
            if (matchItem == null)
            {
                _SelectedOrderItems.Add(new OrderItem
                {
                    Name = item.Name,
                    Count = 1,
                    Price = item.Price,
                });
            }
            else
            {
                matchItem.Count++;
                matchItem.Price += item.Price;
            }
        }

        public void RemoveSelectedOrderItem(Product item)
        {
            var matchItem = _SelectedOrderItems.FirstOrDefault(x => x.Name == item.Name);
            if (matchItem != null)
            {
                if (matchItem.Count == 1)
                {
                    _SelectedOrderItems.Remove(matchItem);
                }
                else
                {
                    matchItem.Count--;
                    matchItem.Price -= item.Price;
                }
            }
        }

        public void ClearSelectedOrderItems()
        {
            _SelectedOrderItems.Clear();
            ResetTotalPrice();
        }
        #endregion

        #region TotalPrice 관련 함수
        public void IncTotalPrice(int price)
        {
            TotalPrice += price;
        }

        public void DecTotalPrice(int price)
        {
            TotalPrice -= price;
        }

        private void ResetTotalPrice()
        {
            TotalPrice = 0;
        }
        #endregion

        public void SetCurrentScreen(KioskScreenEnum screen)
        {
            PreviousScreen = CurrentScreen;
            CurrentScreen = screen;
        }
    }
}
