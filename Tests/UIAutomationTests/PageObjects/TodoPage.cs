using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;

namespace UIAutomationTests.PageObjects
{
    public class TodoPage : BasePage
    {
        public TodoPage(AutomationBase automation, Window window) : base(automation, window) { }

        public TextBox? InputField => FindTextBox("TxtInput");
        public Button? AddButton => FindButton("BtnAdd");
        public ListBox? TodoList => FindListBox("TodoList");

        public void AddTodoItem(string title)
        {
            if (InputField != null) InputField.Text = title;
            AddButton?.Click();
            System.Threading.Thread.Sleep(500);
        }

        public int GetTodoCount() => TodoList?.Items.Length ?? 0;

        public void ClickFirstItemCheckbox()
        {
            var firstItem = TodoList?.Items[0];
            var checkbox = firstItem?.FindFirstDescendant(cf => cf.ByControlType(ControlType.CheckBox));
            checkbox?.AsCheckBox()?.Click();
        }
    }
}
