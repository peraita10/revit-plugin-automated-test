using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using Autodesk.Revit.Attributes;



[Transaction(TransactionMode.Manual)]
public class AssertProdLibInsertion : IExternalCommand
{
    public Result Execute(ExternalCommandData data, ref string msg, ElementSet els)
    {
        var doc = data.Application.ActiveUIDocument.Document;

        // Ajusta a tus familias/tipos esperados
        var fm = new FilteredElementCollector(doc)
                 .OfClass(typeof(Family))
                 .Cast<Family>()
                 .FirstOrDefault(f => f.Name.Contains("ProdLib"));

        var symbols = fm?.GetFamilySymbolIds()
                         .Select(id => doc.GetElement(id))
                         .OfType<FamilySymbol>()
                         .ToList() ?? new List<FamilySymbol>();

        var instances = new FilteredElementCollector(doc)
                        .OfClass(typeof(FamilyInstance))
                        .Cast<FamilyInstance>()
                        .Where(fi => fi.Symbol.Family.Name.Contains("ProdLib"))
                        .ToList();

        var ok = fm != null && symbols.Count > 0 && instances.Count > 0;

        var payload = new
        {
            ok,
            family = fm?.Name,
            symbolCount = symbols.Count,
            instanceCount = instances.Count
        };

        System.IO.Directory.CreateDirectory(System.IO.Path.Combine(
            System.IO.Path.GetTempPath(), "revit-e2e"));
        var path = System.IO.Path.Combine(
            System.IO.Path.GetTempPath(), "revit-e2e", "result.json");
        System.IO.File.WriteAllText(path, Newtonsoft.Json.JsonConvert.SerializeObject(payload));

        return ok ? Result.Succeeded : Result.Failed;
    }
}

