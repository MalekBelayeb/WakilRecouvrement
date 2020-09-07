using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web;

namespace WakilRecouvrement.Web.Models
{
    public class LotViewModel
    {
        [DisplayName("Importer un lot")]
        [Required(ErrorMessage = "Please select file")]
        [FileExtensions(Extensions = ".xls,.xlsx", ErrorMessage = "Only excel file")]
        public HttpPostedFileBase LotFile { get; set; }

    }



}