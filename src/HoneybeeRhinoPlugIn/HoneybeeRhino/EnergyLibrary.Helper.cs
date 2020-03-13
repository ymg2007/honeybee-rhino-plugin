using System;
using System.Collections.Generic;
using System.Linq;
using HB = HoneybeeSchema;

namespace HoneybeeRhino
{
    public static partial class EnergyLibrary
    {
        public static HB.OpaqueConstructionAbridged GetOpaqueConstructionByName(string name)
        {
            var lib = EnergyLibrary.StandardsOpaqueConstructions;
            var obj = lib.FirstOrDefault(_ => _.Name == name);
            if (obj == null)
                throw new ArgumentNullException($"Failed to find the opaque construction {name}");
            return obj;
        }
        public static HB.WindowConstructionAbridged GetWindowConstructionByName(string name)
        {
            var lib = EnergyLibrary.StandardsWindowConstructions;
            var obj = lib.FirstOrDefault(_ => _.Name == name);
            if (obj == null)
                throw new ArgumentNullException($"Failed to find the window construction {name}");
            return obj;
        }
        public static HB.IEnergyMaterial GetOpaqueMaterialByName(string name)
        {
            var lib = EnergyLibrary.StandardsOpaqueMaterials;
            var obj = lib.FirstOrDefault(_ => _.Name == name);
            if (obj == null)
                throw new ArgumentNullException($"Failed to find the opaque material {name}");
            return obj;
        }

        public static HB.IEnergyWindowMaterial GetWindowMaterialByName(string name)
        {
            var lib = EnergyLibrary.StandardsWindowMaterials;
            var obj = lib.FirstOrDefault(_ => _.Name == name);
            if (obj == null)
                throw new ArgumentNullException($"Failed to find the opaque material {name}");
            return obj;
        }

        public static List<HB.IEnergyMaterial> GetConstructionMaterials(HB.OpaqueConstructionAbridged construction)
        {
            return construction.Layers.Select(_ => GetOpaqueMaterialByName(_)).ToList();
        }
        public static List<HB.IEnergyWindowMaterial> GetConstructionMaterials(HB.WindowConstructionAbridged construction)
        {
            return construction.Layers.Select(_ => GetWindowMaterialByName(_)).ToList();
        }

        public static void AddEnergyMaterial(this HB.Model model, List<HB.IEnergyMaterial> materials)
        {
            foreach (var material in materials)
            {
                var exist = model.Properties.Energy.Materials.Any(_ => _ == material);
                if (exist)
                    continue;

                switch (material)
                {
                    case HB.EnergyMaterial em:
                        model.Properties.Energy.Materials.Add(em);
                        break;
                    case HB.EnergyMaterialNoMass em:
                        model.Properties.Energy.Materials.Add(em);
                        break;

                    default:
                        break;
                }



            }
        }

        public static void AddEnergyWindowMaterial(this HB.Model model, List<HB.IEnergyWindowMaterial> materials)
        {
            foreach (var material in materials)
            {
                var exist = model.Properties.Energy.Materials.Any(_ => _ == material);
                if (exist)
                    continue;

                switch (material)
                {
                    
                    case HB.EnergyWindowMaterialBlind em:
                        model.Properties.Energy.Materials.Add(em);
                        break;
                    case HB.EnergyWindowMaterialGas em:
                        model.Properties.Energy.Materials.Add(em);
                        break;
                    case HB.EnergyWindowMaterialGasCustom em:
                        model.Properties.Energy.Materials.Add(em);
                        break;
                    case HB.EnergyWindowMaterialGasMixture em:
                        model.Properties.Energy.Materials.Add(em);
                        break;
                    case HB.EnergyWindowMaterialGlazing em:
                        model.Properties.Energy.Materials.Add(em);
                        break;
                    case HB.EnergyWindowMaterialShade em:
                        model.Properties.Energy.Materials.Add(em);
                        break;
                    case HB.EnergyWindowMaterialSimpleGlazSys em:
                        model.Properties.Energy.Materials.Add(em);
                        break;
                    default:
                        break;
                }



            }
        }

        public static void AddConstructions(this HB.Model model, List<HB.OpaqueConstructionAbridged> constructions)
        {
            foreach (var item in constructions)
            {
                var exist = model.Properties.Energy.Constructions.Any(_ => _ == item);
                if (exist)
                    continue;

                model.Properties.Energy.Constructions.Add(item);

            }
        }
        public static void AddConstructions(this HB.Model model, List<HB.WindowConstructionAbridged> constructions)
        {
            foreach (var item in constructions)
            {
                var exist = model.Properties.Energy.Constructions.Any(_ => _ == item);
                if (exist)
                    continue;

                model.Properties.Energy.Constructions.Add(item);

            }
        }

        public static void AddConstructions(this HB.Model model, List<HB.ShadeConstruction> constructions)
        {
            foreach (var item in constructions)
            {
                var exist = model.Properties.Energy.Constructions.Any(_ => _ == item);
                if (exist)
                    continue;

                model.Properties.Energy.Constructions.Add(item);

            }
        }
        public static void AddConstructions(this HB.Model model, List<HB.AirBoundaryConstructionAbridged> constructions)
        {
            foreach (var item in constructions)
            {
                var exist = model.Properties.Energy.Constructions.Any(_ => _ == item);
                if (exist)
                    continue;

                model.Properties.Energy.Constructions.Add(item);

            }
        }


    }
}
