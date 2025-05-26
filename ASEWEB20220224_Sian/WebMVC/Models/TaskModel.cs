using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
namespace ASEWEB.Models
{
    public class TaskModel
    {

        public TaskModel()
        {

        }
        public List<string> Task_string()
        {
            var S = new ASEWEB.Models.SQLContext();
            return S.Task_Context();
        }
    }
}