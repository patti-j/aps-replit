using DevExpress.Blazor;

using Microsoft.IdentityModel.Tokens;

using ReportsWebApp.Components;
using ReportsWebApp.DB.Models;
using ReportsWebApp.DB.Models.WebApp;

namespace ReportsWebApp.Helpers
{
    public static class GridHelpers
    {
        /// <summary>
        /// Provides some additional context for the standard editor modal based on the record being modified.
        /// Makes some assumptions:
        /// (1) The entity type has a readable type name
        /// (2) The entity overrides ToString with a readable value (like all BaseEntity implementors do)
        /// (3) The editor is only used for standard Creates/Updates, using a Popup-type editor (not inline-grid)
        /// </summary>
        /// <param name="a_e"></param>
        /// <param name="a_gridRef"></param>
        public static void CustomizeEditorTitle(GridCustomizeEditModelEventArgs a_e, IGrid? a_gridRef)
        {
            string typeName = GetTypeName(a_e.EditModel);

            a_gridRef.BeginUpdate();

            string title = a_e.IsNew ?
                $"Create New {typeName}" :
                $"Edit  {typeName} '{a_e.EditModel}'";

            a_gridRef.PopupEditFormHeaderText = title;

            a_gridRef.EndUpdate();
        }

        /// <summary>
        /// Attempt to pull out human-friendly entity name from EditModel
        /// </summary>
        /// <param name="editModel"></param>
        /// <returns></returns>
        private static string GetTypeName(object editModel)
        {
            string typeName;
            try
            {
                typeName = editModel is BaseEntity entity ?
                    entity.TypeDisplayName :
                    editModel?.GetType().Name ?? "Record";
            }
            catch (Exception e)
            {
                typeName = "New Record";
            }

            return typeName;
        }

        /// <summary>
        /// Creates a confirmation dialog and presents it to the user.
        /// If the user accepts, the provided method is run, otherwise nothing happens
        /// </summary>
        /// <param name="a_item">The BaseEntity that should be passed to the method on confirmation</param>
        /// <param name="a_method">The method that should be run if the user accepts</param>
        /// <param name="a_dialog">The ConfirmationDialog box that should be used to display the confirmation</param>
        /// <param name="a_name">A string that should be displayed in place of BaseEntity.Name</param>
        /// <param name="a_action">A string that should be displayed in place of "delete" in "You are about to delete the...".Name</param>
        /// <param name="a_additionalText">A string containing additional text that should be displayed on the modal.".Name</param>
        /// <returns></returns>
        public static async Task ConfirmBeforeCalling(INamedEntity a_item, Func<object, Task> a_method, ConfirmationDialog a_dialog, string? a_name = null, string? a_action = null, string? a_additionalText = null)
        {
            if (a_action == "delete" || a_action == null)
            {
                if (await a_dialog.ConfirmDeleteOperation("Are you sure?", $"You are about to delete the {a_item.TypeDisplayName} '{a_name ?? a_item.Name}'. Are you sure you want to continue?", a_name ?? a_item.Name))
                {
                    await a_method(a_item);
                }
            }
            else
            {
                if (await a_dialog.ConfirmOperation("Are you sure?", $"You are about to {a_action} the {a_item.TypeDisplayName} '{a_name ?? a_item.Name}'. {(a_additionalText.IsNullOrEmpty() ? "" : a_additionalText + " ")}Are you sure you want to continue?"))
                {
                    await a_method(a_item);
                }
            }
            
            
        }
    }
}
