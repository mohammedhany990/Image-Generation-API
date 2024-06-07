using ImageGenerator.Core.Entities.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageGenerator.Core.Entities.Identity
{
    public class AppUser: IdentityUser
    {
        public string Name { get; set; }
        public string Location { get; set; }
        public string ImageName { get; set; }
        public DateTime Birthday { get; set; }
        [NotMapped]
        public List<string>ImagesNames { get; set; } = new List<string>();
        //public List<ImageGeneration> Images { get; set; } = new List<ImageGeneration>();
    }


}
