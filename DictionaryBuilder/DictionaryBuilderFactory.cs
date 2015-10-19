using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
namespace Tonic.DictionaryBuilder
{
    public class DictionaryBuilderFactory
    {
        const string ReflectionDictionaryField = "reflectionField_24EF8F9F_BD0F_49C3_B41C_EC2105100709";


        public static object Create(IProperties Data)
        {
            var appDomain = Thread.GetDomain();
            FieldBuilder ReflectionDictionary;
            var Type = CreateTypeBuilder(appDomain, out ReflectionDictionary);

            foreach (var D in Data.Keys)
            {
                DeclareProperty(Type, D, Data.GetValueType(D), ReflectionDictionary);
            }

            var TypeResult = Type.CreateType();
            var InstanceResult = Activator.CreateInstance(Type);


            //Set the reflection field:
            TypeResult.GetField(ReflectionDictionaryField).SetValue(InstanceResult, Data);

            return InstanceResult;
        }


        private static TypeBuilder CreateTypeBuilder(AppDomain appDomain, out FieldBuilder ReflectionDictionary)
        {
            var assemblyName = new AssemblyName("TonicDictionaryBuilderAssembly");
            var assemblyBuilder = appDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            var moduleBuilder = assemblyBuilder.DefineDynamicModule("TonicDictionaryBuilderModule");
            return CreateAdapterType(moduleBuilder, out ReflectionDictionary);
        }

        private static TypeBuilder CreateAdapterType(ModuleBuilder moduleBuilder, out FieldBuilder ReflectionDictionary)
        {
            var PostName = Guid.NewGuid().ToString().Replace('-', '_');
            var typeBuilder = moduleBuilder.DefineType("TonicDictionaryBuilderType" + PostName,
                TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.BeforeFieldInit);

            ReflectionDictionary = typeBuilder.DefineField(ReflectionDictionaryField, typeof(IProperties), FieldAttributes.Public);

            return typeBuilder;
        }

        private static void DeclareProperty(TypeBuilder typeBuilder, string Name, Type type, FieldBuilder ReflectionDictionary)
        {
            var propertyBuilder = typeBuilder.DefineProperty(Name, PropertyAttributes.None, type, null);
            CreatePropertyGetMethod(typeBuilder, propertyBuilder, ReflectionDictionary);
            CreatePropertySetMethod(typeBuilder, propertyBuilder, ReflectionDictionary);
        }

        const MethodAttributes propertyMethodAttributes = MethodAttributes.Public | MethodAttributes.SpecialName |
                                                  MethodAttributes.HideBySig;

        static readonly MethodInfo ReflectionGet = typeof(IProperties).GetMethod(nameof(IProperties.GetValue));
        static readonly MethodInfo ReflectionSet = typeof(IProperties).GetMethod(nameof(IProperties.SetValue));

        static void CreatePropertyGetMethod(TypeBuilder typeBuilder, PropertyBuilder propertyBuilder, FieldBuilder ReflectionDictionary)
        {

            var getMethodBuilder = typeBuilder.DefineMethod(
                "get_" + propertyBuilder.Name,
                propertyMethodAttributes,
                propertyBuilder.PropertyType,
                Type.EmptyTypes);

            var il = getMethodBuilder.GetILGenerator();


            //return this.ReflectionDictionary.GetValue( PropertyName )
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, ReflectionDictionary);
            il.Emit(OpCodes.Ldstr, propertyBuilder.Name);
            il.Emit(OpCodes.Callvirt, ReflectionGet);
            il.Emit(OpCodes.Unbox_Any, propertyBuilder.PropertyType);
            il.Emit(OpCodes.Ret);

            propertyBuilder.SetGetMethod(getMethodBuilder);
        }

        static void CreatePropertySetMethod(TypeBuilder typeBuilder, PropertyBuilder propertyBuilder, FieldBuilder ReflectionDictionary)
        {
            var getMethodBuilder = typeBuilder.DefineMethod(
                     "set_" + propertyBuilder.Name,
                     propertyMethodAttributes,
                     null,
                     new Type[] { propertyBuilder.PropertyType });

            var il = getMethodBuilder.GetILGenerator();


            //return this.ReflectionDictionary.GetValue( PropertyName )
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, ReflectionDictionary);
            il.Emit(OpCodes.Ldstr, propertyBuilder.Name);
            il.Emit(OpCodes.Ldarg_1);
            if (propertyBuilder.PropertyType.IsValueType )
            {
                il.Emit(OpCodes.Box, propertyBuilder.PropertyType);
            }
            il.Emit(OpCodes.Callvirt, ReflectionSet);
            il.Emit(OpCodes.Ret);

            propertyBuilder.SetSetMethod(getMethodBuilder);
        }
    }
}
