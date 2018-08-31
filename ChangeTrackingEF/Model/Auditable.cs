﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChangeTrackingEF.Model
{
    public class Auditable<T> : IAuditable
    {
        [ScaffoldColumn(false)]
        public DateTime CreatedDate { get; set; }
        [ScaffoldColumn(false)]
        public int CreatedBy { get; set; }
        [ScaffoldColumn(false)]
        public DateTime? UpdatedDate { get; set; }
        [ScaffoldColumn(false)]
        public int? UpdatedBy { get; set; }
        [ScaffoldColumn(false)]
        public bool IsDeleted { get; set; }
    }
}
