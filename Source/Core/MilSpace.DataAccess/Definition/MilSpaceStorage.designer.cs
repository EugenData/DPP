﻿#pragma warning disable 1591
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace MilSpace.DataAccess.Definition
{
	using System.Data.Linq;
	using System.Data.Linq.Mapping;
	using System.Data;
	using System.Collections.Generic;
	using System.Reflection;
	using System.Linq;
	using System.Linq.Expressions;
	using System.ComponentModel;
	using System;
	
	
	[global::System.Data.Linq.Mapping.DatabaseAttribute(Name="MilSpaceApp")]
	public partial class MilSpaceStorageContext : System.Data.Linq.DataContext
	{
		
		private static System.Data.Linq.Mapping.MappingSource mappingSource = new AttributeMappingSource();
		
    #region Extensibility Method Definitions
    partial void OnCreated();
    partial void InsertMilSp_Session(MilSp_Session instance);
    partial void UpdateMilSp_Session(MilSp_Session instance);
    partial void DeleteMilSp_Session(MilSp_Session instance);
    partial void InsertMilSp_Profile(MilSp_Profile instance);
    partial void UpdateMilSp_Profile(MilSp_Profile instance);
    partial void DeleteMilSp_Profile(MilSp_Profile instance);
    #endregion
		
		public MilSpaceStorageContext() : 
				base(global::MilSpace.DataAccess.Properties.Settings.Default.MilSpaceAppConnectionString, mappingSource)
		{
			OnCreated();
		}
		
		public MilSpaceStorageContext(string connection) : 
				base(connection, mappingSource)
		{
			OnCreated();
		}
		
		public MilSpaceStorageContext(System.Data.IDbConnection connection) : 
				base(connection, mappingSource)
		{
			OnCreated();
		}
		
		public MilSpaceStorageContext(string connection, System.Data.Linq.Mapping.MappingSource mappingSource) : 
				base(connection, mappingSource)
		{
			OnCreated();
		}
		
		public MilSpaceStorageContext(System.Data.IDbConnection connection, System.Data.Linq.Mapping.MappingSource mappingSource) : 
				base(connection, mappingSource)
		{
			OnCreated();
		}
		
		internal System.Data.Linq.Table<MilSp_Session> MilSp_Sessions
		{
			get
			{
				return this.GetTable<MilSp_Session>();
			}
		}
		
		internal System.Data.Linq.Table<MilSp_Profile> MilSp_Profiles
		{
			get
			{
				return this.GetTable<MilSp_Profile>();
			}
		}
	}
	
	[global::System.Data.Linq.Mapping.TableAttribute(Name="dbo.MilSp_Session")]
	internal partial class MilSp_Session : INotifyPropertyChanging, INotifyPropertyChanged
	{
		
		private static PropertyChangingEventArgs emptyChangingEventArgs = new PropertyChangingEventArgs(String.Empty);
		
		private string _userName;
		
		private int _ProfileId;
		
		private EntityRef<MilSp_Profile> _MilSp_Profile;
		
    #region Extensibility Method Definitions
    partial void OnLoaded();
    partial void OnValidate(System.Data.Linq.ChangeAction action);
    partial void OnCreated();
    partial void OnuserNameChanging(string value);
    partial void OnuserNameChanged();
    partial void OnProfileIdChanging(int value);
    partial void OnProfileIdChanged();
    #endregion
		
		public MilSp_Session()
		{
			this._MilSp_Profile = default(EntityRef<MilSp_Profile>);
			OnCreated();
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_userName", DbType="NVarChar(50) NOT NULL", CanBeNull=false, IsPrimaryKey=true)]
		public string userName
		{
			get
			{
				return this._userName;
			}
			set
			{
				if ((this._userName != value))
				{
					this.OnuserNameChanging(value);
					this.SendPropertyChanging();
					this._userName = value;
					this.SendPropertyChanged("userName");
					this.OnuserNameChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_ProfileId", DbType="Int NOT NULL", IsPrimaryKey=true)]
		public int ProfileId
		{
			get
			{
				return this._ProfileId;
			}
			set
			{
				if ((this._ProfileId != value))
				{
					if (this._MilSp_Profile.HasLoadedOrAssignedValue)
					{
						throw new System.Data.Linq.ForeignKeyReferenceAlreadyHasValueException();
					}
					this.OnProfileIdChanging(value);
					this.SendPropertyChanging();
					this._ProfileId = value;
					this.SendPropertyChanged("ProfileId");
					this.OnProfileIdChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.AssociationAttribute(Name="MilSp_Profile_MilSp_Session", Storage="_MilSp_Profile", ThisKey="ProfileId", OtherKey="idRow", IsForeignKey=true)]
		public MilSp_Profile MilSp_Profile
		{
			get
			{
				return this._MilSp_Profile.Entity;
			}
			set
			{
				MilSp_Profile previousValue = this._MilSp_Profile.Entity;
				if (((previousValue != value) 
							|| (this._MilSp_Profile.HasLoadedOrAssignedValue == false)))
				{
					this.SendPropertyChanging();
					if ((previousValue != null))
					{
						this._MilSp_Profile.Entity = null;
						previousValue.MilSp_Sessions.Remove(this);
					}
					this._MilSp_Profile.Entity = value;
					if ((value != null))
					{
						value.MilSp_Sessions.Add(this);
						this._ProfileId = value.idRow;
					}
					else
					{
						this._ProfileId = default(int);
					}
					this.SendPropertyChanged("MilSp_Profile");
				}
			}
		}
		
		public event PropertyChangingEventHandler PropertyChanging;
		
		public event PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void SendPropertyChanging()
		{
			if ((this.PropertyChanging != null))
			{
				this.PropertyChanging(this, emptyChangingEventArgs);
			}
		}
		
		protected virtual void SendPropertyChanged(String propertyName)
		{
			if ((this.PropertyChanged != null))
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
	}
	
	[global::System.Data.Linq.Mapping.TableAttribute(Name="dbo.MilSp_Profile")]
	internal partial class MilSp_Profile : INotifyPropertyChanging, INotifyPropertyChanged
	{
		
		private static PropertyChangingEventArgs emptyChangingEventArgs = new PropertyChangingEventArgs(String.Empty);
		
		private int _idRow;
		
		private string _ProfileName;
		
		private string _ProfileData;
		
		private bool _Shared;
		
		private string _Creator;
		
		private System.DateTime _Created;
		
		private EntitySet<MilSp_Session> _MilSp_Sessions;
		
    #region Extensibility Method Definitions
    partial void OnLoaded();
    partial void OnValidate(System.Data.Linq.ChangeAction action);
    partial void OnCreated();
    partial void OnidRowChanging(int value);
    partial void OnidRowChanged();
    partial void OnProfileNameChanging(string value);
    partial void OnProfileNameChanged();
    partial void OnProfileDataChanging(string value);
    partial void OnProfileDataChanged();
    partial void OnSharedChanging(bool value);
    partial void OnSharedChanged();
    partial void OnCreatorChanging(string value);
    partial void OnCreatorChanged();
    partial void OnCreatedChanging(System.DateTime value);
    partial void OnCreatedChanged();
    #endregion
		
		public MilSp_Profile()
		{
			this._MilSp_Sessions = new EntitySet<MilSp_Session>(new Action<MilSp_Session>(this.attach_MilSp_Sessions), new Action<MilSp_Session>(this.detach_MilSp_Sessions));
			OnCreated();
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_idRow", DbType="Int NOT NULL", IsPrimaryKey=true)]
		public int idRow
		{
			get
			{
				return this._idRow;
			}
			set
			{
				if ((this._idRow != value))
				{
					this.OnidRowChanging(value);
					this.SendPropertyChanging();
					this._idRow = value;
					this.SendPropertyChanged("idRow");
					this.OnidRowChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_ProfileName", DbType="NChar(50) NOT NULL", CanBeNull=false)]
		public string ProfileName
		{
			get
			{
				return this._ProfileName;
			}
			set
			{
				if ((this._ProfileName != value))
				{
					this.OnProfileNameChanging(value);
					this.SendPropertyChanging();
					this._ProfileName = value;
					this.SendPropertyChanged("ProfileName");
					this.OnProfileNameChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_ProfileData", DbType="Text", UpdateCheck=UpdateCheck.Never)]
		public string ProfileData
		{
			get
			{
				return this._ProfileData;
			}
			set
			{
				if ((this._ProfileData != value))
				{
					this.OnProfileDataChanging(value);
					this.SendPropertyChanging();
					this._ProfileData = value;
					this.SendPropertyChanged("ProfileData");
					this.OnProfileDataChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Shared", DbType="Bit NOT NULL")]
		public bool Shared
		{
			get
			{
				return this._Shared;
			}
			set
			{
				if ((this._Shared != value))
				{
					this.OnSharedChanging(value);
					this.SendPropertyChanging();
					this._Shared = value;
					this.SendPropertyChanged("Shared");
					this.OnSharedChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Creator", DbType="NChar(20) NOT NULL", CanBeNull=false)]
		public string Creator
		{
			get
			{
				return this._Creator;
			}
			set
			{
				if ((this._Creator != value))
				{
					this.OnCreatorChanging(value);
					this.SendPropertyChanging();
					this._Creator = value;
					this.SendPropertyChanged("Creator");
					this.OnCreatorChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Created", DbType="DateTime NOT NULL")]
		public System.DateTime Created
		{
			get
			{
				return this._Created;
			}
			set
			{
				if ((this._Created != value))
				{
					this.OnCreatedChanging(value);
					this.SendPropertyChanging();
					this._Created = value;
					this.SendPropertyChanged("Created");
					this.OnCreatedChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.AssociationAttribute(Name="MilSp_Profile_MilSp_Session", Storage="_MilSp_Sessions", ThisKey="idRow", OtherKey="ProfileId")]
		public EntitySet<MilSp_Session> MilSp_Sessions
		{
			get
			{
				return this._MilSp_Sessions;
			}
			set
			{
				this._MilSp_Sessions.Assign(value);
			}
		}
		
		public event PropertyChangingEventHandler PropertyChanging;
		
		public event PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void SendPropertyChanging()
		{
			if ((this.PropertyChanging != null))
			{
				this.PropertyChanging(this, emptyChangingEventArgs);
			}
		}
		
		protected virtual void SendPropertyChanged(String propertyName)
		{
			if ((this.PropertyChanged != null))
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
		
		private void attach_MilSp_Sessions(MilSp_Session entity)
		{
			this.SendPropertyChanging();
			entity.MilSp_Profile = this;
		}
		
		private void detach_MilSp_Sessions(MilSp_Session entity)
		{
			this.SendPropertyChanging();
			entity.MilSp_Profile = null;
		}
	}
}
#pragma warning restore 1591
