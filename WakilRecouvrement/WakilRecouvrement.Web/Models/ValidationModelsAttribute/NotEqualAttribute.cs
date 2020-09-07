using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WakilRecouvrement.Web.Models.ValidationModelsAttribute
{
    public class NotEqualAttribute:ValidationAttribute
    {
        private string OtherAttribute;
        public NotEqualAttribute(string otherAttribute)
        {
            this.OtherAttribute = otherAttribute;
        }
  
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var otherPropretyInfo = validationContext.ObjectType.GetProperty(OtherAttribute);
            var otherValue = otherPropretyInfo.GetValue(validationContext.ObjectInstance);

            if(value.ToString().Equals(otherValue.ToString()))
            {
                return new ValidationResult(string.Format(ErrorMessage, validationContext.MemberName, otherValue));
            }else
            {
                return ValidationResult.Success;
            }
        }

    }
}