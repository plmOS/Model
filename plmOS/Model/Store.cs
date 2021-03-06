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
using System.Reflection;
using System.ComponentModel;

namespace plmOS.Model
{
    public class Store : INotifyPropertyChanged, IDisposable
    {
        public Auth.IManager Auth { get; private set; }

        public Database.ISession Database { get; private set; }

        public Logging.Log Log { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(String Name)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs(Name));
            }
        }

        private Boolean _writing;
        public Boolean Writing
        {
            get
            {
                return this._writing;
            }
            private set
            {
                if (this._writing != value)
                {
                    this._writing = value;
                    this.OnPropertyChanged("Writing");
                }
            }
        }

        private Int32 _writingTotal;
        public Int32 WritingTotal
        {
            get
            {
                return this._writingTotal;
            }
            private set
            {
                if (this._writingTotal != value)
                {
                    this._writingTotal = value;
                    this.OnPropertyChanged("WritingTotal");
                }
            }
        }

        private Int32 _writingNumber;
        public Int32 WritingNumber
        {
            get
            {
                return this._writingNumber;
            }
            private set
            {
                if (this._writingNumber != value)
                {
                    this._writingNumber = value;
                    this.OnPropertyChanged("WritingNumber");
                }
            }
        }

        private Boolean _reading;
        public Boolean Reading
        {
            get
            {
                return this._reading;
            }
            private set
            {
                if (this._reading != value)
                {
                    this._reading = value;
                    this.OnPropertyChanged("Reading");
                }
            }
        }

        private Int32 _readingTotal;
        public Int32 ReadingTotal
        {
            get
            {
                return this._readingTotal;
            }
            private set
            {
                if (this._readingTotal != value)
                {
                    this._readingTotal = value;
                    this.OnPropertyChanged("ReadingTotal");
                }
            }
        }

        private Int32 _readingNumber;
        public Int32 ReadingNumber
        {
            get
            {
                return this._readingNumber;
            }
            private set
            {
                if (this._readingNumber != value)
                {
                    this._readingNumber = value;
                    this.OnPropertyChanged("ReadingNumber");
                }
            }
        }

        private Boolean _initialised;
        public Boolean Initialised
        {
            get
            {
                return this._initialised;
            }
            private set
            {
                if (this._initialised != value)
                {
                    this._initialised = value;
                    this.OnPropertyChanged("Initialised");
                }
            }
        }

        void Database_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch(e.PropertyName)
            {
                case "Reading":
                    this.Reading = this.Database.Reading;
                    break;
                case "ReadingTotal":
                    this.ReadingTotal = this.Database.ReadingTotal;
                    break;
                case "ReadingNumber":
                    this.ReadingNumber = this.Database.ReadingNumber;
                    break;
                case "Writing":
                    this.Writing = this.Database.Writing;
                    break;
                case "WritingTotal":
                    this.WritingTotal = this.Database.WritingTotal;
                    break;
                case "WritingNumber":
                    this.WritingNumber = this.Database.WritingNumber;
                    break;
                case "Initialised":
                    this.Initialised = this.Database.Initialised;
                    break;
                default:
                    throw new NotImplementedException("Database Property Change not Implemeted: " + e.PropertyName);
            }
        }

        private Dictionary<String, ItemType> AllItemTypeCache;

        private Dictionary<String, ItemType> ItemTypeCache;

        private Dictionary<String, RelationshipType> RelationshipTypeCache;

        internal ItemType AllItemType(String Name)
        {
            return this.AllItemTypeCache[Name];
        }

        public ItemType ItemType(String Name)
        {
            return this.ItemTypeCache[Name];
        }

        public ItemType ItemType(Type Type)
        {
            return this.ItemType(Type.FullName);
        }

        public IEnumerable<ItemType> ItemTypes
        {
            get
            {
                return this.ItemTypeCache.Values;
            }
        }

        private void AddItemTypeToCache(Type Type)
        {
            ItemType itemtype = new ItemType(this, Type);
            this.AllItemTypeCache[itemtype.Name] = itemtype;
            this.ItemTypeCache[itemtype.Name] = itemtype;
        }

        private void AddRelationshipTypeToCache(Type Type)
        {
            RelationshipType reltype = new RelationshipType(this, Type);
            this.AllItemTypeCache[reltype.Name] = reltype;
            this.RelationshipTypeCache[reltype.Name] = reltype;
        }

        private void LoadItemTypes()
        {
            // Load All ItemTypes
            foreach (ItemType itemtype in this.AllItemTypeCache.Values)
            {
                itemtype.Load();
            }

            // Create ItemTypes and RelationshipTypes in Database
            foreach (ItemType itemtype in this.AllItemTypeCache.Values)
            {
                itemtype.Create();
            }
        }

        private Dictionary<String, List> ListCache;

        private void AddListTypeToCache(Type Type)
        {
            this.ListCache[Type.FullName] = (List)Activator.CreateInstance(Type, new object[] {});
        }

        public IEnumerable<String> ListNames
        {
            get
            {
                return this.ListCache.Keys;
            }
        }

        public List List(String Name)
        {
            return this.ListCache[Name];
        }
        
        public void LoadAssembly(String AssemblyFilename)
        {
            this.LoadAssembly(new FileInfo(AssemblyFilename));
        }

        public void LoadAssembly(FileInfo AssemblyFile)
        {
            Assembly assembly = Assembly.LoadFrom(AssemblyFile.FullName);

            // Find all ItemTypes and Relationships

            foreach (Type type in assembly.GetTypes())
            {
                if (type.IsSubclassOf(typeof(Relationship)))
                {
                    this.AddRelationshipTypeToCache(type);
                }
                else if (type.IsSubclassOf(typeof(Item)))
                {
                    this.AddItemTypeToCache(type);
                }
                else if (type.IsSubclassOf(typeof(List)))
                {
                    this.AddListTypeToCache(type);
                }
            }

            // Load RelationshipTypes
            this.LoadItemTypes();
        }

        public Session Login(Auth.ICredentials Credentials)
        {
            return new Session(this, this.Auth.Login(Credentials));
        }

        public void Dispose()
        {
            if (this.Database != null)
            {
                this.Database.Dispose();
            }
        }

        public Store(Auth.IManager Auth, Database.ISession Database, Logging.Log Log)
        {
            this.Reading = false;
            this.ReadingNumber = 0;
            this.ReadingTotal = 0;
            this.Writing = false;
            this.WritingNumber = 0;
            this.WritingTotal = 0;
            this.Initialised = false;

            this.Auth = Auth;
            this.Database = Database;
            this.Log = Log;
            this.Database.PropertyChanged += Database_PropertyChanged;
            this.AllItemTypeCache = new Dictionary<String, ItemType>();
            this.ItemTypeCache = new Dictionary<String, ItemType>();
            this.RelationshipTypeCache = new Dictionary<String, RelationshipType>();
            this.ListCache = new Dictionary<String, List>();
            
            // Load Base ItemTypes
            this.AddItemTypeToCache(typeof(Item));
            this.AddItemTypeToCache(typeof(File));
            this.AddRelationshipTypeToCache(typeof(Relationship));
            this.LoadItemTypes();
        }
    }
}
