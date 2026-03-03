using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Kiosk.CommonEnum;

namespace Kiosk.ViewModels
{
    public class CategoryViewModel : ViewModelBase
    {
        public Dictionary<CategoryEnum, CategoryOption> CategoryList { get; set; }

        public Command SelectCategoryCommand { get; set; }

        public CategoryViewModel()
        {
            CategoryList = new Dictionary<CategoryEnum, CategoryOption>();
            InitCategory();
            SelectCategoryCommand = new Command(SelectCategory);

            Messenger.Instance.Subscribe<CategoryEnum>(MessengerEnum.ChangeCategory, this, OnCategoryChanged);
        }

        private void InitCategory()
        {
            var array = Enum.GetValues(typeof(CategoryEnum));
            foreach (CategoryEnum cat in array)
            {
                var option = new CategoryOption
                {
                    IsSelected = cat == 0,      // 첫번째 카테고리는 기본으로 선택된 상태
                    Name = cat.GetDescription()
                };
                CategoryList.Add(cat, option);
            }

            SelectCategory(array.GetValue(0));
        }

        private void SelectCategory(object obj)
        {
            if (obj is CategoryEnum category)
            {
                Messenger.Instance.Send(MessengerEnum.SelectCategory, category);
            }
        }

        private void OnCategoryChanged(CategoryEnum category)
        {
            var selectedCategory = CategoryList.FirstOrDefault(x => x.Value.IsSelected).Value;
            selectedCategory.IsSelected = false;

            CategoryList[category].IsSelected = true;
            SelectCategory(category);
        }
    }

    public class CategoryOption : ViewModelBase
    {
        public string Name { get; set; }

        private bool _IsSelected;
        public bool IsSelected { get => _IsSelected; set => SetValue(ref _IsSelected, value); }
    }
}
