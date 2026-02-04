using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using FlaUI.Core.Input;
using FlaUI.Core.WindowsAPI;
using FlaUI.UIA3;
using NUnit.Framework;
using System;
using System.Drawing;
using System.IO;



namespace TestProject1
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void ProdLib_Insert_Object_And_Verify()
        {
            // 1) Lanzar Revit con el modelo
            var app = Application.Launch(@"C:\Program Files\Autodesk\Revit 2025\Revit.exe", @"C:\tests\ModeloBase.rvt");
            using var automation = new UIA3Automation();
            var main = app.GetMainWindow(automation, TimeSpan.FromMinutes(2));

            // 2) Activar pestaña "ProdLib"
            var ribbonTab = main.FindFirstDescendant(cf => cf.ByName("ProdLib").And(cf.ByControlType(ControlType.TabItem))).AsTabItem();
            ribbonTab.Select();

            // 3) Abrir librería y navegar (ajusta nombres reales de tu librería/producto)
            var libList = main.FindFirstDescendant(cf => cf.ByName("Libraries")).AsListBox();
            var myLib = libList.Items.First(i => i.Name.Contains("MiLibreria"));
            myLib.DoubleClick();

            var productTree = main.FindFirstDescendant(cf => cf.ByAutomationId("ProdLibTree")).AsTree();
            var node = productTree.FindFirstDescendant(cf => cf.ByName("Producto XYZ")).AsTreeItem();
            node.DoubleClick(); // o botón Insert/Load

            // 4) Colocar instancia con un clic en el lienzo (vista activa)
            var point = new Point(400, 300);
            Mouse.MoveTo(point);
            Mouse.Click(MouseButton.Left);

            // 5) Ejecutar el Asserter (p.ej. atajo Ctrl+Alt+A)
            Keyboard.TypeSimultaneously(VirtualKeyShort.CONTROL, VirtualKeyShort.ALT, VirtualKeyShort.KEY_A);


            // 6) Leer result.json
            var resultPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "revit-e2e", "result.json");
            dynamic json = Newtonsoft.Json.JsonConvert.DeserializeObject(System.IO.File.ReadAllText(resultPath));

            Assert.That((bool)json.ok,Is.True,$"Falló la inserción: {System.IO.File.ReadAllText(resultPath)}");

            // 7) Cerrar el panel/librería (opcional) y Revit
            ribbonTab = main.FindFirstDescendant(cf => cf.ByName("Architecture")).AsTabItem(); // o cerrar panel ProdLib
            ribbonTab.Select();
            main.Close(); // maneja prompt de guardar si aplica
        }
    }
}