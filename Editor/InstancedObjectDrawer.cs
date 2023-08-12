using UnityEngine;
using System;
using System.Reflection;
using UnityEditor;
using System.Linq;
using UnityEditor.IMGUI.Controls;

namespace InstancedObject.Editor
{
    [CustomPropertyDrawer(typeof(InstancedObjectAttribute))]
    public class InstancedObjectDrawer : PropertyDrawer{
        private Type[] implementations;
        private int implementationTypeIndex;
        private SerializedProperty targetProperty;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            // Random size ^^
            return EditorGUI.GetPropertyHeight(property, label, true) + EditorGUIUtility.singleLineHeight * 6; 
        }
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            string GetTypeName(string typeName)
            {
                int splitIndex = typeName.IndexOf(' ');
                return typeName.Substring(splitIndex + 1);
            }

            Assembly GetAssembly(string typeName)
            {
                int splitIndex = typeName.IndexOf(' ');
                return Assembly.Load(typeName.Substring(0, splitIndex + 1));
            }

            void NewLine(ref Rect rect)
            {
                rect.y += EditorGUIUtility.singleLineHeight;
            }
            
            if (property.propertyType != SerializedPropertyType.ManagedReference)
            {
                return;
            }

            EditorGUI.BeginProperty(position,label,property);
            
            string typeName = property.managedReferenceFieldTypename;
            var assembly = GetAssembly(typeName);
            Type baseType = assembly.GetType(GetTypeName(typeName));
            
            Rect fieldPosition = new Rect(position);
            fieldPosition.width -= EditorGUIUtility.labelWidth;
            fieldPosition.height = EditorGUIUtility.singleLineHeight;
            fieldPosition.y += fieldPosition.height + EditorGUI.GetPropertyHeight(property, label, true);
            
            if (implementations == null || GUI.Button(fieldPosition,"Refresh implementations"))
            {
                implementations = GetImplementations(baseType).Where(impl => !impl.IsSubclassOf(typeof(UnityEngine.Object))).ToArray();
            }

            NewLine(ref fieldPosition);
            implementationTypeIndex = EditorGUI.Popup(fieldPosition, "Implementations", implementationTypeIndex, implementations.Select(impl => impl.FullName).ToArray());

            NewLine(ref fieldPosition);
            
            EditorGUI.BeginChangeCheck(); 
            targetProperty = property;
            if (GUI.Button(fieldPosition, "Create instance"))
            {
                var instance = Activator.CreateInstance(implementations[implementationTypeIndex]);

                targetProperty.serializedObject.Update();
                targetProperty.managedReferenceValue = instance;
                targetProperty.isExpanded = true;
                targetProperty.serializedObject.ApplyModifiedProperties();
            }
            label.text = GetTypeName(property.managedReferenceFullTypename);
            
            EditorGUI.PropertyField(position,property,label,true);
            EditorGUI.EndProperty();
        }

        private static Type[] GetImplementations(Type baseType)
        {
            var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(assembly => assembly.GetTypes());

            return types.Where(p => baseType.IsAssignableFrom(p) && !p.IsAbstract).ToArray();
        }
    }
}