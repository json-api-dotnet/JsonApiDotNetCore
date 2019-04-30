using System;
using System.Collections.Generic;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCoreExample.Models
{
    public class TodoItemMeta : Identifiable
    {


        /// <summary>
        /// some field to store arbitrary metadata in json or xml format
        /// </summary>
        [Attr("meta-data")]
        public virtual string MetaData { get; set; }


        [HasOne("todo-item")]
        public virtual TodoItem TodoItem { get; set; }

    }
}
