using Kiosk.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Kiosk.CommonEnum;

namespace Kiosk.ViewModels
{
    public class MenuViewModel : ViewModelBase
    {
        public ObservableCollection<MenuItemVM> DisplayProducts { get; set; }        // 현재 view에 전시되는 물품 목록

        private readonly Dictionary<CategoryEnum, List<MenuItemVM>> _Products;    // 현재 뷰모델에서 가지고 있는 모든 물품 목록
        private const int _MaxDisplayCount = 8;
        private int _PageIndex;
        private CategoryEnum _SelectedCategory;

        public Command PrevButtonCommand { get; set; }
        public Command NextButtonCommand { get; set; }

        public MenuViewModel()
        {
            DisplayProducts = new ObservableCollection<MenuItemVM>();
            _Products = new Dictionary<CategoryEnum, List<MenuItemVM>>();
            PrevButtonCommand = new Command(PrevButton);
            NextButtonCommand = new Command(NextButton);

            InitProducts();

            Messenger.Instance.Subscribe<CategoryEnum>(MessengerEnum.SelectCategory, this, SetCategory);
            Messenger.Instance.Subscribe(MessengerEnum.MenuPrev, this, (object obj) => { PrevButton(); });
            Messenger.Instance.Subscribe(MessengerEnum.MenuNext, this, (object obj) => { NextButton(); });
        }

        private void InitProducts()
        {
            try
            {
                var allProducts = DataManager.instance.GetAllProducts();
                foreach (var item in allProducts)
                {
                    var vm = new MenuItemVM
                    {
                        Product = item
                    };

                    var category = (CategoryEnum)Enum.Parse(typeof(CategoryEnum), item.Category);
                    if (!_Products.ContainsKey(category))
                        _Products.Add(category, new List<MenuItemVM>());

                    _Products[category].Add(vm);
                }
            }
            catch (Exception ex)
            {
                FileLogger.Log(ex);
            }
        }

        private void SetCategory(CategoryEnum category)
        {
            _SelectedCategory = category;
            _PageIndex = 0;
            DisplayItem();
        }

        private void DisplayItem()
        {
            try
            {
                DisplayProducts.Clear();
                if (_Products.TryGetValue(_SelectedCategory, out var list) && list.Count > 0)
                {
                    int firstDisplayIndex = _PageIndex * _MaxDisplayCount;
                    int lastDisplayIndex = (_PageIndex + 1) * _MaxDisplayCount;
                    for (int i = firstDisplayIndex; i < lastDisplayIndex; i++)
                    {
                        if (i >= list.Count)
                            break;

                        DisplayProducts.Add(list[i]);
                    }
                }
            }
            catch (Exception ex)
            {
                FileLogger.Log(ex);
            }
        }

        private void PrevButton()
        {
            if (_PageIndex > 0)
            {
                _PageIndex--;
                DisplayItem();
            }
        }

        private void NextButton()
        {
            if (_Products.TryGetValue(_SelectedCategory, out var list))
            {
                int lastDisplayIndex = (_PageIndex + 1) * _MaxDisplayCount;
                if (lastDisplayIndex < list.Count)
                {
                    _PageIndex++;
                    DisplayItem();
                }
            }
        }
    }

    public class MenuItemVM : ViewModelBase
    {
        public Product Product { get; set; }

        public Command SelectItemCommand { get; set; }

        public MenuItemVM()
        {
            SelectItemCommand = new Command(SelectItem);
        }

        private void SelectItem(object obj)
        {
            if (obj is Product product)
            {
                Messenger.Instance.Send(MessengerEnum.SelectItem, product);
            }
        }
    }
}
