using System;
using System.Windows.Forms;
using CxFlatUI;
using CxFlatUI.Controls;
using FirstAidQuiz.Controllers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FirstAidQuizTests {

    [TestClass]
    public class UnitTest1 {

        [TestMethod]
        public void Resulttest() {
            var ListViewController = new ListViewController();
            try {
                var form = new Form();
                var groupBox = new CxFlatGroupBox();
                var groupButton = new CxFlatButton();
                groupBox.Controls.Add(groupButton);
                form.Controls.Add(groupBox);
                ListViewController.CheckShouldEnableSubmitButton(form);
                ListViewController.AddSelectedOptionForAnswer(groupButton, new EventArgs());
            } catch (Exception ex) {
                throw ex;
            }

            Assert.IsInstanceOfType(ListViewController, typeof(ListViewController));

            if (ListViewController != null) {
                Assert.IsInstanceOfType(ListViewController, typeof(ListViewController));
            }
        }
    }
}