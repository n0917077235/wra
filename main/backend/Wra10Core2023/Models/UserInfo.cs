using System.ComponentModel.DataAnnotations;

namespace Wra10Core2023.Models
{
    public class UserInfo
    {
        [Required]
        public string UserID { get; set; }
        public string UserName { get; set; }
        public string UserGroupID { get; set; }
        public string Belongs { get; set; }
        public int SpecialUser { get; set; }
        public int EventNotice { get; set; }
        public string EMail { get; set; }
        //public bool Flag { get; set; }
        public bool Enabled { get; set; }
    }

    public class ResetPasswordModel
    {
        [Required]
        public string UserID { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }
        [Required]
        public string Token { get; set; }
    }

    public class UserGroup
    {
        public string UserGroupID { get; set; }
        public string UserGroupName { get; set; }
      
    }
}
