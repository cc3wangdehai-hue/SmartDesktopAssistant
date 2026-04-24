using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.UIA3;

namespace UIAutomationTests.PageObjects
{
    /// <summary>
    /// Page Object for Todo Widget
    /// </summary>
    public class TodoPage : BasePage
    {
        public TodoPage(AutomationBase automation, Window window) : base(automation, window) { }

        // Elements
        public TextBox? InputField => FindTextBox("TxtInput");
        public Button? AddButton => FindButton("BtnAdd");
        public ListBox? TodoList => FindListBox("TodoList");
        public Label? EmptyState => FindLabel("EmptyState");

        // Actions
        public void AddTodoItem(string title)
        {
            InputField!.Text = title;
            AddButton!.Click();
            System.Threading.Thread.Sleep(500);
        }

        public void AddTodoItemWithEnter(string title)
        {
            InputField!.Text = title;
            InputField.Focus();
            // Simulate Enter key
            Window.Focus();
            System.Threading.Thread.Sleep(100);
        }

        public int GetTodoCount()
        {
            return TodoList?.Items.Length ?? 0;
        }

        public void ClickFirstItemCheckbox()
        {
            var firstItem = TodoList?.Items[0];
            var checkbox = firstItem?.FindFirstDescendant(cf => cf.ByControlType(ControlType.CheckBox));
            checkbox?.AsCheckBox()?.Click();
        }

        public void DeleteFirstItem()
        {
            var deleteButton = TodoList?.Items[0]?.FindFirstDescendant(cf => cf.ByName("删除"));
            deleteButton?.AsButton()?.Click();
        }
    }
}
