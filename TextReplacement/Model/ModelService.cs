using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
namespace TextReplacement
{
    public class ModelService
    {
        public  Document doc;
        public Selection sel;
        public ModelService(UIApplication app)
        {
            doc = app.ActiveUIDocument.Document;
            sel = app.ActiveUIDocument.Selection;
        }
        public List<BuiltInCategory> categories = new List<BuiltInCategory>()
        {
            BuiltInCategory.OST_ProjectInformation,
            BuiltInCategory.OST_TextNotes,
            BuiltInCategory.OST_Views,
            BuiltInCategory.OST_Sheets,
            BuiltInCategory.OST_GenericModel,
            BuiltInCategory.OST_Walls,
            BuiltInCategory.OST_Floors,
            BuiltInCategory.OST_StructuralFraming,
            BuiltInCategory.OST_StructuralColumns,
            BuiltInCategory.OST_Rebar,
            BuiltInCategory.OST_StructuralFoundation,
            BuiltInCategory.OST_Stairs,
            BuiltInCategory.OST_Ramps,
            BuiltInCategory.OST_Assemblies,
            BuiltInCategory.OST_SpecialityEquipment,
            BuiltInCategory.OST_MechanicalEquipment,
            BuiltInCategory.OST_Columns,
            BuiltInCategory.OST_Windows,
            BuiltInCategory.OST_Rooms,
            BuiltInCategory.OST_GenericAnnotation,
            BuiltInCategory.OST_DetailComponents,
            BuiltInCategory.OST_Schedules,
            BuiltInCategory.OST_Coupler,
            BuiltInCategory.OST_Roofs,
            BuiltInCategory.OST_Dimensions
        };
        public List<string> parameterNames = new List<string>()
        {
            "NPP_Номер_Комплекта",
            "NPP_Номер_Комплекта_2",
            "NPP_Произвольный_Текст",
            "Комментарии",
            "Марка",
            "NPP_Code",
            "NPP_Ссылка_На_Лист",
            "NPP_Название_Комплекта",
            "Номер",
            "NPP_Текст_2",
            "NPP_Текст_1",
            "NPP_Название_Комплекта_Перевод",
            "NPP_Марка_Сборного_Изделия",
            "NPP_Фильтрация_Модели",
            "NPP_Номер_Комплекта_Армирование",
            "Имя листа"
        };
        public Category GetCategory(BuiltInCategory builtInCategory)
        {
            return Category.GetCategory(doc, builtInCategory);
        }
        public List<Category> GetOtherCategories(List<BuiltInCategory> exeptCats)
        {
            var allCats = doc.Settings.Categories;
            var exeptBicInts = exeptCats.Select(exeptBic => (int)exeptBic).ToList();
            var otherCats = new List<Category>();
            foreach (Category cat in allCats)
            {
                if (!exeptBicInts.Contains(cat.Id.IntegerValue))
                {
                    if (cat.AllowsBoundParameters)
                    {
                        otherCats.Add(cat);
                    }
                }
            }
            otherCats = (from cat in otherCats orderby cat.Name select cat).ToList();
            return otherCats;
        }
        public ObservableCollection<CategoryWrap> GetCategoryWraps(List<BuiltInCategory> builtInCategories)
        {
            ObservableCollection<CategoryWrap> categoryWraps = new ObservableCollection<CategoryWrap>();
            var scheduleCat = GetCategory(BuiltInCategory.OST_Schedules);
            categoryWraps.Add(new CategoryWrap(scheduleCat, BuiltInCategory.OST_Schedules) { NameToDisplay = "Заголовки таблиц" });
            categoryWraps.Add(new CategoryWrap(GetCategory(BuiltInCategory.OST_Dimensions), BuiltInCategory.OST_Dimensions) { NameToDisplay = "Размеры текстом" });
            var exceptBics = new List<BuiltInCategory>() { BuiltInCategory.OST_Dimensions};            
            for (int i = 0; i < builtInCategories.Count; i++)
            {
                if (exceptBics.Contains(builtInCategories[i]))
                {
                    continue;
                }
                Category category;
                category = GetCategory(builtInCategories[i]);
                categoryWraps.Add(new CategoryWrap(category, builtInCategories[i]));                                
            }            
            return categoryWraps;
        }
        public ObservableCollection<CategoryWrap> GetCategoryWraps(List<Category> categories)
        {
            ObservableCollection<CategoryWrap> categoryWraps = new ObservableCollection<CategoryWrap>();
            foreach(Category category in categories)
            {
                categoryWraps.Add(new CategoryWrap(category));
            }
            return categoryWraps;
        }
        public ObservableCollection<ParameterWrap> GetParameterWraps(List<string> parameterNames)
        {
            ObservableCollection<ParameterWrap> parameterWraps = new ObservableCollection<ParameterWrap>();
            for (int i = 0; i < parameterNames.Count; i++)
            {                
                parameterWraps.Add(new ParameterWrap(parameterNames[i]));
            }
            return parameterWraps;
        }
        public void ReplaceText(List<CategoryWrap> categoryWraps, ObservableCollection<ParameterWrap> parameterWraps, string oldValue, string newValue, bool excludeGroupItems, ParameterWrap selectedFilterParam, string selectedFilterParamValue)
        {
            Transaction transaction = new Transaction(doc);
            transaction.Start("Замена текста");
            var checkedCatWraps = categoryWraps.Where(catWrap => catWrap.IsChecked).ToList();
            var checkedParamNames = parameterWraps.Where(parameterWrap => parameterWrap.IsChecked).Select(checkedParameter => checkedParameter.Name).ToList();
            var checkedCats = checkedCatWraps.Select(checkedCatWrap => checkedCatWrap.Category).ToList();
            
            for (int i = 0; i < checkedCatWraps.Count; i++)
            {
                var checkedCategoryWrap = checkedCatWraps[i];
                ChangeText(oldValue, newValue, checkedCategoryWrap, excludeGroupItems, doc, selectedFilterParam, selectedFilterParamValue, checkedParamNames);                             
            }
            transaction.Commit();
        }
        private void ChangeText(string oldValuesStr, string newValuesStr, CategoryWrap checkedCategoryWrap, bool excludeGroupItems, Document doc, ParameterWrap selectedFilterParam, string selectedFilterParamValue,List<string> checkedParamNames)
        {
            string[] oldValues = oldValuesStr.Split(new char[] { ';' });
            string[] newValues = newValuesStr.Split(new char[] { ';' });
            if (oldValues.Length != newValues.Length)
            {
                throw new Exception("Количество замяемых значений должно быть равным количеству значений для замены!");
            }
            for (int i = 0; i < oldValues.Length; i++)
            {
                var oldValue = oldValues[i];
                var newValue = newValues[i];
                if (checkedCategoryWrap.BuiltInCategory == BuiltInCategory.OST_TextNotes)
                {
                    ChangeTextNotes(oldValue, newValue, excludeGroupItems);

                }
                else if (checkedCategoryWrap.NameToDisplay == "Заголовки таблиц")
                {
                    ChangeTableHeads(oldValue, newValue);
                }

                else if (checkedCategoryWrap.NameToDisplay == "Размеры текстом")
                {
                    ChangeTextDimensions(oldValue, newValue, excludeGroupItems);
                }
                else //Change elements from other checked categories
                {
                    var elementsFilteredByCat = new FilteredElementCollector(doc).OfCategoryId(checkedCategoryWrap.Category.Id).WhereElementIsNotElementType().ToList();
                    if (selectedFilterParam != null)
                    {
                        if (!string.IsNullOrEmpty(selectedFilterParamValue))
                        {
                            var selectedFilterParamName = selectedFilterParam.Name;
                            elementsFilteredByCat = elementsFilteredByCat.
                                Where(element => element.LookupParameter(selectedFilterParamName) != null).
                                Where(element => !element.LookupParameter(selectedFilterParamName).IsReadOnly).
                                Where(element => element.LookupParameter(selectedFilterParamName).StorageType.ToString() == "String").
                                Where(element => element.LookupParameter(selectedFilterParamName).AsString() != null).
                                Where(element => element.LookupParameter(selectedFilterParamName).AsString() == selectedFilterParamValue).
                                ToList();
                        }
                    }
                    for (int j = 0; j < checkedParamNames.Count; j++)
                    {
                        var checkedParamName = checkedParamNames[j];
                        var elements = elementsFilteredByCat.
                            Where(element => element.LookupParameter(checkedParamName) != null).
                            Where(element => !element.LookupParameter(checkedParamName).IsReadOnly).
                            Where(element => element.LookupParameter(checkedParamName).StorageType.ToString() == "String").
                            Where(element => element.LookupParameter(checkedParamName).AsString() != null).
                            ToList();
                        if (excludeGroupItems)
                        {
                            elements = elements.Where(element => element.GroupId.IntegerValue == -1/*null*/).ToList();
                        }
                        for (int k = 0; k < elements.Count; k++)
                        {
                            var newParamValue = elements[k].LookupParameter(checkedParamName).AsString().Replace(oldValue, newValue);
                            elements[k].LookupParameter(checkedParamName).Set(newParamValue);
                        }
                    }
                }
            }

        }
        private void ChangeTextDimensions(string oldValue, string newValue, bool excludeGroupItems)
        {
            var textDims = new FilteredElementCollector(doc).
                OfCategory(BuiltInCategory.OST_Dimensions).
                WhereElementIsNotElementType().
                Select(element => element as Dimension).
                ToList();
            if (excludeGroupItems)
            {
                textDims = textDims.Where(textNote => textNote.GroupId.IntegerValue == -1/*null*/).ToList();
            }
            for (int i = 0; i < textDims.Count; i++)
            {
                if (textDims[i].ValueOverride != null)
                {
                    if (textDims[i].ValueOverride.Contains(oldValue))
                    {
                        var newTextValue = textDims[i].ValueOverride.Replace(oldValue, newValue);
                        textDims[i].ValueOverride = newTextValue;
                    }
                }

            }
        }       
        public void ChangeTextNotes(string oldValue, string newValue, bool excludeGroupItems)
        {
            var textNotes = new FilteredElementCollector(doc).
                OfCategory(BuiltInCategory.OST_TextNotes).
                WhereElementIsNotElementType().
                Select(element => element as TextNote).                
                ToList();
            if (excludeGroupItems)
            {
                textNotes = textNotes.Where(textNote => textNote.GroupId.IntegerValue == -1/*null*/).ToList();
            }
            for (int i = 0; i < textNotes.Count; i++)
            {
                var newTextValue = textNotes[i].Text.Replace(oldValue, newValue);
                textNotes[i].Text = newTextValue;
            }
        }
        public void ChangeTableHeads(string oldValue, string newValue)
        {
            var scheduleViews = new FilteredElementCollector(doc).
                OfCategory(BuiltInCategory.OST_Schedules).
                WhereElementIsNotElementType().
                Select(element => element as ViewSchedule).
                ToList();
            for (int i = 0; i < scheduleViews.Count; i++)
            {
                var tableData = scheduleViews[i].GetTableData();
                var sectionData = tableData.GetSectionData(SectionType.Header);
                var lastColNumber = sectionData.LastColumnNumber;
                var lastRowNumber = sectionData.LastRowNumber;
                for (int rowNum = 0; rowNum <= lastRowNumber; rowNum++)
                {
                    var cellText = sectionData.GetCellText(rowNum, 0);
                    if (!string.IsNullOrEmpty(cellText))
                    {
                        var newCellText = cellText.Replace(oldValue, newValue);
                        if(newCellText != cellText)
                        {
                            sectionData.SetCellText(rowNum, 0, newCellText);
                        }
                    }
                }
            }
        }
    }
    public class ParameterWrap: CheckBoxWrapper
    {
        public string Name { get;private set; }
        public ParameterWrap(string name)
        {
            Name = name;    
            NameToDisplay = name.Replace("_", "__");
        }
    }
    public class CategoryWrap : CheckBoxWrapper
    {
        public CategoryWrap(Category category, BuiltInCategory builtInCategory = BuiltInCategory.INVALID)
        {
            NameToDisplay = category.Name;
            Category = category;
            BuiltInCategory = builtInCategory;
        }
        public Category Category { get; private set; }
        public BuiltInCategory BuiltInCategory { get; private set; }
    }
    public abstract class CheckBoxWrapper : ViewModelBase
    {
        private bool isChecked;
        public bool IsChecked
        {
            get { return isChecked; }
            set { isChecked = value; OnPropertyChanged(); }
        }
        public string NameToDisplay { get; set; }
    }
}