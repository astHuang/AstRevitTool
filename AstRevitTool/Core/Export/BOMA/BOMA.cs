using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using AstRevitTool.Core.Export;

namespace AstRevitTool.Core.Export
{
    public class BOMA
    {
        private int Boundary_area;
        private double floor_rentable;
        private decimal floor_allocation_ratio;
        private decimal load_factor;
        private Level level;

        public double rentable_exclusion;
        public string space_id;
        public double retail_area;
        public double tenant_area;
        public double tenant_ancillary_area;
        public double _occupant_area;
        public double building_amenity_area;
        public double building_service_area;
        public double _floor_usable_area;
        public double floor_service_area;

        public List<BOMA_cell> cells;

        public class BOMA_cell:BOMA
        {

            public void Modify(BOMA_cell target)
            {
                if (!target.space_id.Equals(this.space_id)) return;
                this.rentable_exclusion += target.rentable_exclusion;
                this.tenant_area += target.tenant_area;
            }
            public BOMA_cell(SvgExport.locationJson area)
            {
                this.space_id = area.space_id;
                this.rentable_exclusion = area.boma_exclusion.Equals("0") ? 0 : area.area;
                string category = area.name;
                switch (category)
                {
                    case "Retail":
                        this.retail_area = area.area;
                        break;
                    case "Tenant Area":
                        this.tenant_area = area.area;
                        break;
                    case "Tenant Ancillary Area":
                        this.tenant_ancillary_area = area.area;
                        break;
                    case "Building Amenity Area":
                        this.building_amenity_area = area.area;
                        break;
                    case "Building Service Area":
                        this.building_service_area = area.area;
                        break;
                    case "Floor Service Area":
                        this.floor_service_area = area.area;
                        break;

                }
                this._occupant_area = tenant_area + tenant_ancillary_area + retail_area;
                this._floor_usable_area = _occupant_area + building_amenity_area;

            }

        }

        
        public BOMA()
        {

        }

        public BOMA(SvgExport.floorJson floorData, Level l)
        {
            this.Boundary_area = floorData.Value[0].boundaryArea;
            this.level = l;
        }

        public void Compute_Floor()
        {

        }

    }
}
