using QLLMChat.Models.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace QLLMChat.Helpers
{
    public class ChatRoleDataManager
    {
        public static readonly ChatRoleDataManager Default = new("./Roles");
        //public record class RoleDataAbilityModel(String Name, String Description, String Example);


        private string BaseDirectory = "./Roles";
        private ChatRoleDataModel[] RoleDatas = null;
        public ChatRoleDataManager(string DirectoryPath = "./Roles")
        {
            this.BaseDirectory = DirectoryPath;
            Directory.CreateDirectory(this.BaseDirectory);
        }
        public ChatRoleDataModel? GetRole(string Name)
        {
            return GetRoles().FirstOrDefault(r => r.Name == Name);
        }
        public IEnumerable<ChatRoleDataModel> GetRoles()
        {
            if (RoleDatas != null) return RoleDatas;
            else return (RoleDatas = query().ToArray());
            IEnumerable<ChatRoleDataModel> query()
            {
                foreach (var d in Directory.GetDirectories(BaseDirectory))
                {
                    DirectoryInfo directoryInfo = new DirectoryInfo(d);
                    string name = directoryInfo.Name;
                    string mainText = "";

                    foreach (var f in directoryInfo.GetFiles())
                    {
                        if (f.Name.ToLower() == "main.txt")
                        {
                            string text = File.ReadAllText(f.FullName);
                            mainText = text;
                        }
                        else
                        {

                        }
                    }
                    yield return new ChatRoleDataModel(name, mainText);
                }
            }
        }
    }
}
