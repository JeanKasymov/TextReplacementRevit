using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace TextReplacement
{
    public class ViewModel : ViewModelBase
    {
        public ModelService modelService;
        public ViewModel(ModelService modelService)
        {
            this.modelService = modelService;
            CategoryWraps = modelService.GetCategoryWraps(modelService.categories);
            OtherCategoryWraps = modelService.GetCategoryWraps(modelService.GetOtherCategories(modelService.categories));
            ParameterWraps = modelService.GetParameterWraps(modelService.parameterNames);
        }
        #region Команды
        public ICommand PluginMainCommand => new RelayCommandWithoutParameter(ReplaceText);
        private void ReplaceText()
        {
            var allCatWraps = CategoryWraps.ToList();
            allCatWraps.AddRange(OtherCategoryWraps);
            modelService.ReplaceText(allCatWraps, ParameterWraps, OldValue, NewValue, ExcludeGroupElements, SelectedFilterParam, SelectedFilterParamValue);
        }
        public ICommand CatsCheckedChangedCommand => new RelayCommand<object>(CatsCheckedChanged); 
        private void CatsCheckedChanged(object obj)
        {
            var chekbox = obj as System.Windows.Controls.CheckBox;
            if (chekbox != null)
            {
                if (chekbox.IsChecked == true)
                {
                    foreach (var categoryWrap in CategoryWraps)
                    {
                        categoryWrap.IsChecked = true;
                    }
                }
                else
                {
                    foreach (var categoryWrap in CategoryWraps)
                    {
                        categoryWrap.IsChecked = false;
                    }
                }
            }
        }
        public ICommand ParamsCheckedChangedCommand => new RelayCommand<object>(ParamsCheckedChanged);
        private void ParamsCheckedChanged(object obj)
        {
            var chekbox = obj as System.Windows.Controls.CheckBox;
            if (chekbox != null)
            {
                if (chekbox.IsChecked == true)
                {
                    foreach (var parameterWrap in ParameterWraps)
                    {
                        parameterWrap.IsChecked = true;
                    }
                }
                else
                {
                    foreach (var parameterWrap in ParameterWraps)
                    {
                        parameterWrap.IsChecked = false;
                    }
                }
            }
        }
        public ICommand AddParameterCommand => new RelayCommandWithoutParameter(AddParameter);

        private void AddParameter()
        {
            if (!string.IsNullOrEmpty(NewParameter))
            {
                if (!ParameterWraps.Select(wrap=>wrap.Name).Contains(NewParameter))
                {
                    ParameterWraps.Add(new ParameterWrap(NewParameter));
                }
            }
        }

        #endregion
        #region Свойства
        private ObservableCollection<CategoryWrap> otherCategoryWraps;
        public ObservableCollection<CategoryWrap> OtherCategoryWraps
        {
            get => otherCategoryWraps;
            set { otherCategoryWraps = value; OnPropertyChanged(); }
        }
        private ObservableCollection<CategoryWrap> categoryWraps;
        public ObservableCollection<CategoryWrap> CategoryWraps
        {
            get => categoryWraps;
            set { categoryWraps = value; OnPropertyChanged(); }
        }
        private ObservableCollection<ParameterWrap> parameterWraps;
        public ObservableCollection<ParameterWrap> ParameterWraps
        {
            get { return parameterWraps; }
            set { parameterWraps = value; OnPropertyChanged(); }
        }
        private string oldValue;
        public string OldValue
        {
            get { return oldValue; }
            set { oldValue = value; OnPropertyChanged(); }
        }
        private string newValue;
        public string NewValue
        {
            get { return newValue; }
            set { newValue = value; OnPropertyChanged(); }
        }
        private string newParameter;
        public string NewParameter
        {
            get { return newParameter; }
            set { newParameter = value; OnPropertyChanged(); }
        }
        private bool excludeGroupElements = true;
        public bool ExcludeGroupElements
        {
            get { return excludeGroupElements; }
            set { excludeGroupElements = value; OnPropertyChanged(); }
        }
        private ParameterWrap selectedFilterParam;
        public ParameterWrap SelectedFilterParam
        {
            get { return selectedFilterParam; }
            set { selectedFilterParam = value; OnPropertyChanged(); }
        }
        private string selectedFilterParamValue;
        public string SelectedFilterParamValue
        {
            get { return selectedFilterParamValue; }
            set { selectedFilterParamValue = value; OnPropertyChanged(); }
        }
        #endregion
    }
}
