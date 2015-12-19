using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DatabaseSchemaReader;
using DatabaseSchemaReader.DataSchema;

namespace Kea
{
    public class DbCheckMethods
    {
        static Type GetNetType(DatabaseColumn Col)
        {
            Type ret;
            if (Col.DbDataType == "tinyint")
                ret = typeof(Byte);
            else
                ret = Col.DataType.GetNetType();

            if (Col.Nullable && ret.IsValueType)
            {
                return typeof(Nullable<>).MakeGenericType(ret);
            }
            else
                return ret;
        }


        private static void CheckFK(Action<string> log, DatabaseTable Table, PropertyInfo Column, PropertyInfo Nav)
        {
            if (!Table.FindColumn(Column.Name).IsForeignKey)
            {
                log($"Foreign key for the table {Table}.{Column.Name} doesn't exists");
                return;
            }

        }


        static bool IsNullable(Type T) => T.IsGenericType && T.GetGenericTypeDefinition() == typeof(Nullable<>);
        static Type ToNonNullable(Type T) => IsNullable(T) ? T.GetGenericArguments()[0] : T;

        static bool IsModelType(Type T) => !(T.IsValueType || T == typeof(string));
        static void CheckColumn(Action<string> Log, DatabaseTable Table, PropertyInfo P, Type T)
        {
            try
            {

                var col = Table.FindColumn(P.Name);

                if (col == null)
                {
                    if (IsModelType(P.PropertyType))
                        Log($"La propiedad '{Table.Name}.{P.Name}' debe de ser virtual");
                    else
                        Log($"La columna '{P.Name}' no existe en la tabla '{Table.Name }'");
                    return;
                }
                if (col.Name.ToLowerInvariant().StartsWith("id") && !col.IsForeignKey)
                {
                    Log($"La columna '{Table.Name}.{P.Name}' no es una llave foranea");
                }

                if (col.IsPrimaryKey)
                {
                    if (P.GetCustomAttribute<DatabaseGeneratedAttribute>() != null)
                    {
                        bool HasDefaultValue = !string.IsNullOrEmpty(col.DefaultValue);
                        var Value = P.GetCustomAttribute<DatabaseGeneratedAttribute>().DatabaseGeneratedOption;
                        if (HasDefaultValue && Value == DatabaseGeneratedOption.None)
                        {
                            Log($"La llave primaria {T.Name}.{P.Name} tiene valor por default pero un DatabaseGeneratedOption = None");
                        }
                        else if (!HasDefaultValue && Value == DatabaseGeneratedOption.Identity)
                        {
                            Log($"La llave primaria {T.Name}.{P.Name} no tiene valor por default pero un DatabaseGeneratedOption = Identity");
                        }
                    }
                    else
                        Log($"La llave primaria {T.Name}.{P.Name} debe de tener el atributo DatabaseGenerated y especificar None o Identity");

                }

                if (P.PropertyType == typeof(string))
                {
                    //Check string lenght:
                    var len = col.Length == -1 ? (int?)null : col.Length;
                    var Att = P.GetCustomAttribute<StringLengthAttribute>();
                    if (len != null && (Att == null || Att.MaximumLength != len.Value))
                    {
                        Log($"La propiedad '{T.Name}.{P.Name}' debe de tener el atributo [StringLength({len})]");
                    }
                }
                var ColType = GetNetType(col);
                if (ToNonNullable(P.PropertyType).IsEnum && ToNonNullable(ColType) == typeof(int))
                {
                    if (ColType == typeof(int?) && !IsNullable(P.PropertyType))
                        Log($"La propiedad '{T.Name}.{P.Name}' no acepta nulos pero su columna si");
                    else if (ColType == typeof(int) && IsNullable(P.PropertyType))
                        Log($"La propiedad '{T.Name}.{P.Name}' si acepta nulos pero su columna no");
                }
                else if (P.PropertyType.IsEnum)
                {
                    Log($"Las propiedad de enumeracion '{T.Name}.{P.Name}' debe de ser int");
                }
                else if (ColType != P.PropertyType)
                {
                    if (ToNonNullable(ColType) == ToNonNullable(P.PropertyType))
                    {
                        if (IsNullable(ColType))
                            Log($"La propiedad '{T.Name}.{P.Name}' no acepta nulos pero su columna si");
                        else
                            Log($"La propiedad '{T.Name}.{P.Name}' si acepta nulos pero su columna no");
                    }
                    else
                        Log($"La propiedad '{T.Name}.{P.Name}' tiene un tipo '{P.PropertyType.FullName}' diferente a '{col.DbDataType} {(col.Nullable ? "null" : "")}'");
                }
                else if (ColType == typeof(string))
                {
                    var PropertyReq = P.GetCustomAttribute<RequiredAttribute>() != null;
                    var ColReq = !col.Nullable;
                    if (PropertyReq && !ColReq)
                        Log($"La propiedad '{T.Name}.{P.Name}' es requerida pero su columna no");
                    else if (!PropertyReq && ColReq)
                        Log($"La propiedad '{T.Name}.{P.Name}' es no requerida pero su columna si");
                }
            }
            catch (Exception ex)
            {
                Log("No se pudo verificar la columna " + P.Name + ":" + ex.Message);
            }
        }

        public static async Task Check(Type ContextType, string ConnectionString, string providerName, Action<double> Progress, Action<string> Log)
        {
            var ModelTypes = ContextType.GetProperties()
                .Where(x => x.PropertyType.IsGenericType && x.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>))
                .Select(x => x.PropertyType.GetGenericArguments()[0]);

            await Check(ModelTypes, ConnectionString, providerName,  Progress, Log);
        }
        static async Task Check(IEnumerable<Type> ModelType, string ConnectionString, string providerName, Action<double> Progress, Action<string> log)
        {
            if (Progress == null)
                Progress = x => { };

            var dbReader = new DatabaseReader(ConnectionString, providerName);
            var schema = await Task.Run(() => dbReader.ReadAll());


            Func<PropertyInfo, bool> IsNavProp = P =>
            P.CanRead &&
            P.CanWrite &&
            P.GetGetMethod().IsVirtual &&
            !Tonic.RLinq.IsICollectionOfT(P.PropertyType) &&
            !P.PropertyType.IsValueType &&
            P.PropertyType != typeof(string);
            Func<PropertyInfo, bool> IsFKCollection = P => P.CanRead && P.CanWrite && P.GetGetMethod().IsVirtual && Tonic.RLinq.IsICollectionOfT(P.PropertyType);

            Func<PropertyInfo, bool> IsColumn = P => P.CanRead && P.CanWrite && P.GetCustomAttribute<NotMappedAttribute>() == null && !IsNavProp(P) && !IsFKCollection(P);


            double step = 1.0 / ModelType.Count();
            double progress = 0.0;
            foreach (var T in ModelType)
            {
                try
                {
                    var TableName = T.GetCustomAttribute<TableAttribute>()?.Name;
                    if (TableName == null)
                    {
                        log($"El modelo {T.Name} no tiene el atributo Table");
                        TableName = T.Name;
                    }

                    var Table = schema.FindTableByName(TableName);

                    if (Table.PrimaryKey == null || Table.PrimaryKey.Columns.Count == 0)
                    {
                        log($"La table {T.Name} no tiene llave primaria");
                    }

                    if (Table == null)
                    {
                        log("No existe la tabla " + TableName);
                        continue;
                    }
                    var Props = T.GetProperties();
                    foreach (var P in Props.Where(IsColumn))
                    {
                        await Task.Run(() => CheckColumn(log, Table, P, T));
                    }

                    foreach (var C in Table.Columns)
                    {
                        if (!Props.Any(x => x.Name == C.Name))
                            log($"La columna {Table.Name}.{C.Name} existe en la base de datos pero no en el modelo");
                    }
                    foreach (var P in Props.Where(IsFKCollection))
                    {

                        object ModelInstance;
                        try
                        {
                            ModelInstance = await Task.Run(() => Activator.CreateInstance(T));
                        }
                        catch (Exception ex)
                        {
                            log($"No se pudo crear una instancia del modelo {T.Name }");
                            continue;
                        }

                        var Value = await Task.Run(() => P.GetValue(ModelInstance));
                        if (Value == null)
                            log($"El valor de la coleccion {T.Name}.{P.Name} es nulo");

                        var Type = Tonic.RLinq.GetEnumerableType((IEnumerable)Value);
                        if (!ModelType.Contains(Type))
                            log($"El tipo {Type.Name} no se incluyo en la definicion del modelo, en {T.Name}.{P.Name}");
                    }

                    foreach (var P in Props.Where(IsNavProp))
                    {
                        var Type = P.PropertyType;
                        if (!ModelType.Contains(Type))
                            log($"El tipo {Type.Name} no se incluyo en la definicion del modelo, en {T.Name}.{P.Name}");

                        var IdProp = Props.Where(x => x.Name == "Id" + P.Name).FirstOrDefault();
                        if (IdProp != null)
                        {
                            CheckFK(log, Table, IdProp, P);
                        }

                    }
                }
                catch (Exception ex)
                {
                    log("No se pudo verificar el modelo " + T.Name + ":" + ex.Message);
                }

                progress += step;
                Progress(progress);
            }
        }
    }
}
