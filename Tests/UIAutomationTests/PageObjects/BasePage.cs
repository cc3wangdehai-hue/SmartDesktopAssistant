using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using FlaUI.UIA3;

namespace UIAutomationTests.PageObjects
{
    /// <summary>
    /// Base class for all Page Objects
    /// Implements Page Object Model pattern for cleaner test code
    /// </summary>
    public abstract class BasePage
    {
        protected readonly AutomationBase Automation;
        protected readonly Window Window;

        protected BasePage(AutomationBase automation, Window window)
        {
            Automation = automation;
            Window = window;
        }

        protected Button? FindButton(string automationId)
        {
            return Window.FindFirstDescendant(cf => cf.ByAutomationId(automationId))?.AsButton();
        }

        protected TextBox? FindTextBox(string automationId)
        {
            return Window.FindFirstDescendant(cf => cf.ByAutomationId(automationId))?.AsTextBox();
        }

        protected Label? FindLabel(string automationId)
        {
            return Window.FindFirstDescendant(cf => cf.ByAutomationId(automationId))?.AsLabel();
        }

        protected ListBox? FindListBox(string automationId)
        {
            return Window.FindFirstDescendant(cf => cf.ByAutomationId(automationId))?.AsListBox();
        }

        protected void WaitForElement(string automationId, int timeoutMs = 5000)
        {
            var start = DateTime.Now;
            while ((DateTime.Now - start).TotalMilliseconds < timeoutMs)
            {
                var element = Window.FindFirstDescendant(cf => cf.ByAutomationId(automationId));
                if (element != null) return;
                System.Threading.Thread.Sleep(100);
            }
            throw new TimeoutException($"Element {automationId} not found within {timeoutMs}ms");
        }

        public bool IsVisible => Window.IsAvailable;
        public string Title => Window.Name;
    }
}
