﻿/*  
  plmOS Model provides a .NET client library for managing PLM (Product Lifecycle Management) data.

  Copyright (C) 2015 Processwall Limited.

  This program is free software: you can redistribute it and/or modify
  it under the terms of the GNU Affero General Public License as published
  by the Free Software Foundation, either version 3 of the License, or
  (at your option) any later version.

  This program is distributed in the hope that it will be useful,
  but WITHOUT ANY WARRANTY; without even the implied warranty of
  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
  GNU Affero General Public License for more details.

  You should have received a copy of the GNU Affero General Public License
  along with this program.  If not, see http://opensource.org/licenses/AGPL-3.0.
 
  Company: Processwall Limited
  Address: The Winnowing House, Mill Lane, Askham Richard, York, YO23 3NW, United Kingdom
  Tel:     +44 113 815 3440
  Email:   support@processwall.com
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace plmOS.Model
{
    [Model.ItemTypeID("35E5A1B3-5E52-3482-BC7D-097F1435EFA0")]
    public class File : Item
    {
        const int buffersize = 256;

        [Model.PropertyAttributes.StringProperty(true, true, 255)]
        public String Name { get; private set; }

        public FileStream Read()
        {
            Database.File databasefile = new Database.File(this);
            return this.Session.Store.Database.ReadFromVault(databasefile);
        }

        public void Read(FileInfo File)
        {
            if (!this.LockedForCreate)
            {
                byte[] buffer = new byte[buffersize];
                int bytesread = 0;

                using (FileStream infile = this.Read())
                {
                    using (FileStream outfile = new FileStream(File.FullName, FileMode.Create))
                    {
                        while ((bytesread = infile.Read(buffer, 0, buffersize)) > 0)
                        {
                            outfile.Write(buffer, 0, bytesread);
                        }
                    }
                }
            }
            else
            {
                throw new Exceptions.ItemLockedException(this);
            }
        }

        public FileStream Write(String Name)
        {
            if (this.LockedForCreate)
            {
                this.Name = Name;
                Database.File databasefile = new Database.File(this);
                return this.Session.Store.Database.WriteToVault(databasefile);
            }
            else
            {
                throw new Exceptions.ItemNotLockedException(this);
            }
        }

        public void Write(FileInfo File)
        {
            byte[] buffer = new byte[buffersize];
            int bytesread = 0;

            using (FileStream infile = new FileStream(File.FullName, FileMode.Open))
            {
                using (FileStream outfile = this.Write(File.Name))
                {
                    while ((bytesread = infile.Read(buffer, 0, buffersize)) > 0)
                    {
                        outfile.Write(buffer, 0, bytesread);
                    }
                }
            }
        }

        public void Copy(File File)
        {
            byte[] buffer = new byte[buffersize];
            int bytesread = 0;

            using (FileStream infile = File.Read())
            {
                using (FileStream outfile = this.Write(File.Name))
                {
                    while ((bytesread = infile.Read(buffer, 0, buffersize)) > 0)
                    {
                        outfile.Write(buffer, 0, bytesread);
                    }
                }
            }
        }

        public void Copy(FileStream Stream, String Name)
        {
            byte[] buffer = new byte[buffersize];
            int bytesread = 0;

            using (FileStream outfile = this.Write(Name))
            {
                while ((bytesread = Stream.Read(buffer, 0, buffersize)) > 0)
                {
                    outfile.Write(buffer, 0, bytesread);
                }
            }
        }

        public File(Session Session)
            :base(Session)
        {
   
        }

        public File(Session Session, Database.IFile DatabaseFile)
            :base(Session, DatabaseFile)
        {
            this.Initialise(DatabaseFile);
        }
    }
}
