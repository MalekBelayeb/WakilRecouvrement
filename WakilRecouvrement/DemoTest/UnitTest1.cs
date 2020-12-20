using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WakilRecouvrement.Domain.Entities;
using WakilRecouvrement.Web.Controllers;

namespace DemoTest
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {

            Employe employe = new Employe();
            AuthentificationController authentifcationController = new AuthentificationController();

            employe.Username = "test";
            employe.Password = "pass";
            employe.ConfirmPassword = "pass";
            employe.RoleId = 1;
            

            Assert.AreEqual(authentifcationController.IsValid(employe),true);
        
        }
    }
}
