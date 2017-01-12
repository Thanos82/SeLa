using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace ODataClassesGenerator
{
    public partial class Form1 : Form
    {
        StreamWriter outSLClassFile;

        Boolean generateJS = false;

        string currentOpenFilePath = "";
        private string filesDir = "";

        private Edmx metadata = null;

        static string moduleName = "SAPB1";
        static string serviceCharSeparator = "_";
        static string LoginInfoClassName = "LoginInfo";
        static string B1SLReferenceFileName = @"B1SLReference.";
        static string B1SLProxyFileName = "xsjs/B1SLProxy.xsjs";

        // Keep the list of keys per entity
        System.Collections.Hashtable keysTable;
        private class KeyPair
        {
            public string keyName;
            public string keyType;
        }

        // Keep the parameters of a function
        private class FunctionParams
        {
            public string paramName;
            public string paramType;
        }
        // keep data for a Service Function
        private class ServiceFunction
        {
            public ServiceFunction(string name, FunctionParams[] pars, string ret)
            {
                functionName = name;
                parameters = pars;
                returnType = ret;
            }
            public string functionName;
            public FunctionParams[] parameters;
            public string returnType;
        }

        public Form1()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Action to read oData file on press button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_ReadOData_Click(object sender, EventArgs e)
        {
            currentOpenFilePath = AskFileName();

            ReadODataFile(currentOpenFilePath);
        }

        /// <summary>
        /// Get OData File Path
        /// </summary>
        /// <returns></returns>
        private string AskFileName()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.InitialDirectory = filesDir;
            ofd.Filter = "oData file (*.xml) |*.xml;";
            ofd.RestoreDirectory = true;
            ofd.Multiselect = false;

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                filesDir = ofd.FileName.Substring(0, ofd.FileName.LastIndexOf("\\"));
                return ofd.FileName;
            }
            return "";
        }

        /// <summary>
        /// Read oData file
        /// EntityContainer Name = objecttypes, EntityType = class name
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private bool ReadODataFile(string fileName)
        {
            keysTable = new System.Collections.Hashtable();

            if (fileName == null || fileName.Length == 0 || !File.Exists(fileName))
            {
                System.Windows.Forms.MessageBox.Show("The file " + fileName
                    + " has not been found. Please select a valid log file.",
                    "File not found", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return false;
            }

            // Read output language type
            generateJS = radioButt_JS.Checked;

            FileStream fStream = null;
            string outStr = null;
            StreamReader sr = null;
            System.IO.MemoryStream mStream = null;
            try
            {
                // To be able to open the file while being modified by the addon use FileShare.ReadWrite
                fStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                sr = new StreamReader(fStream);
                outStr = sr.ReadToEnd();

                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(outStr);

                mStream = new System.IO.MemoryStream();
                xmlDoc.Save(mStream);
                System.Xml.Serialization.XmlSerializer serializer = new
                    System.Xml.Serialization.XmlSerializer(typeof(Edmx));
                mStream.Position = 0;
                metadata = (Edmx)serializer.Deserialize(mStream);

                createSLClassFile();

                addStartModule();

                addCallSL();

                int itemsNb = metadata.DataServices.Schema.Items.GetLength(0);

                for (int i = 0; i < itemsNb; i++)
                {
                    if (metadata.DataServices.Schema.Items[i].GetType().Name == "EnumType") 
                        addEnum((EnumType)metadata.DataServices.Schema.Items[i]);

                    else if (metadata.DataServices.Schema.Items[i].GetType().Name == "ComplexType")
                        addComplexClass((ComplexType)metadata.DataServices.Schema.Items[i]);

                    else if (metadata.DataServices.Schema.Items[i].GetType().Name == "EntityType")
                        addEntityClass((EntityType)metadata.DataServices.Schema.Items[i]);

                    else if (metadata.DataServices.Schema.Items[i].GetType().Name == "EntityContainer")
                        addEntityContainer((EntityContainer)metadata.DataServices.Schema.Items[i]);
                }
                addEndModule();

            }
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show("Wrong Xml file format: \"" + e.Message + "\".",
                    "Wrong Xml file format");
                return false;
            }
            finally
            {
                closeSLClassFile();
                if (sr != null) { sr.Close(); sr = null; }
                if (fStream != null) { fStream.Close(); fStream = null; }
                if (mStream != null) { mStream.Close(); mStream = null; }
            }

            if (metadata == null)
            {
                System.Windows.Forms.MessageBox.Show("No data.");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Open output file
        /// </summary>
        private void createSLClassFile()
        {
            string fileName = B1SLReferenceFileName + (generateJS ? "js" : "ts");
            outSLClassFile = new StreamWriter(fileName, false);
            //outSLClassFile.WriteLine("The very first line!");
                
        }

        /// <summary>
        /// Close output file 
        /// </summary>
        private void closeSLClassFile()
        {
            if (outSLClassFile != null)
                outSLClassFile.Close();
        }

        /// <summary>
        /// Create Start Module of SL output file
        /// </summary>
        private void addStartModule()
        {
            if (generateJS)
            {
                //var SAPB1;
                outSLClassFile.WriteLine("var " + moduleName + ";");
                //(function (SAPB1) {
                outSLClassFile.WriteLine("(function (" + moduleName + ") {");
            }
            else
            {
                //module SAPB1 {
                outSLClassFile.WriteLine("module " + moduleName + " {");
            }
        }

        /// <summary>
        /// Create End Module of SL output file
        /// </summary>
        private void addEndModule()
        {
            if (generateJS)
            {
                //})(SAPB1 || (SAPB1 = {}));
                outSLClassFile.WriteLine("})(SAPB1 || (SAPB1 = {}));");
            }
            else
            {
                outSLClassFile.WriteLine("}");
            }
        }

        //JS
        //var AccountCategorySourceEnum = (function () {
        //function AccountCategorySourceEnum() {
        //}
        //AccountCategorySourceEnum.acsBalanceSheet = "acsBalanceSheet";
        //AccountCategorySourceEnum.acsProfitAndLoss = "acsProfitAndLoss";
        //return AccountCategorySourceEnum;
        //}());
        //SAPB1.AccountCategorySourceEnum = AccountCategorySourceEnum;
        //TS
        //export class AccountCategorySourceEnum {
        //static acsBalanceSheet = "acsBalanceSheet";
        //static acsProfitAndLoss = "acsProfitAndLoss";
        //}
        /// <summary>
        /// Add enumeration class
        /// </summary>
        /// <param name="enumDef"></param>
        /// <returns></returns>
        private bool addEnum(EnumType enumDef)
        {
            string memberName;
            string enumName = enumDef.Name;
            int maxNb = enumDef.Member.GetLength(0);

            if (generateJS)
            {
                outSLClassFile.WriteLine("var " + enumName + " = (function() {");
                outSLClassFile.WriteLine("function " + enumName + " () {}");
            }
            else
            {
                outSLClassFile.WriteLine("export class " + enumName + " {");
            }

            for (int j = 0; j < maxNb; j++)
            {
                memberName = enumDef.Member[j].Name;
                if (generateJS)
                {
                    outSLClassFile.WriteLine(enumName + "." + memberName + " = \"" + memberName + "\";");
                }
                else
                {
                    outSLClassFile.WriteLine("static " + memberName + " = \"" + memberName + "\";");
                }
            }

            if (generateJS)
            {
                outSLClassFile.WriteLine("return " + enumName + ";");
                outSLClassFile.WriteLine("}());");
                outSLClassFile.WriteLine(moduleName + "." + enumName + " = " + enumName + ";");
            }
            else
            {
                outSLClassFile.WriteLine("}");
            }
            outSLClassFile.WriteLine("");

            return true;
        }

        /// <summary>
        /// Add ComplexType class
        /// </summary>
        /// <param name="complexType"></param>
        /// <returns></returns>
        private bool addComplexClass(ComplexType complexType)
        {
            string propName;
            string propNullable;
            string propType; 

            string complexTypeName = complexType.Name;

            if (generateJS)
                writeJSClass(complexTypeName);
            else
            {
                outSLClassFile.WriteLine("export class " + complexTypeName + " {");

                int maxNb = complexType.Property.GetLength(0);

                for (int j = 0; j < maxNb; j++)
                {
                    propName = complexType.Property[j].Name;
                    propNullable = complexType.Property[j].Nullable;
                    propType = getTSType(complexType.Property[j].Type);

                    outSLClassFile.WriteLine("  " + propName + ": " + propType + ";");
                }

                outSLClassFile.WriteLine("}");
            }
            outSLClassFile.WriteLine("");

            return true;
        }

        // key
        // 
        // JS
        //var BusinessPartner = (function () {
        //function BusinessPartner() {
        //}
        //return BusinessPartner;
        //}());
        //SAPB1.BusinessPartner = BusinessPartner;
        //
        // TS
        //export class BusinessPartnerParams {
        //CardCode: string;
        //}
        /// <summary>
        /// Add Entity class
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        private bool addEntityClass(EntityType entityType)
        {
            string propName;
            string propNullable;
            string propType;
            KeyPair kp;
            int maxNb = entityType.Property.GetLength(0);
           
            string entityTypeName = entityType.Name;

            // we can have more than 1 key
            // collection of key names
            System.Collections.ArrayList keysArray = new System.Collections.ArrayList();
            for (int i=0; i < entityType.Key.Length; i++)
            {
                keysArray.Add(entityType.Key[i].Name);
            }

            if (generateJS)
            {
                writeJSClass(entityTypeName);
            }
            else
            {
                outSLClassFile.WriteLine("export class " + entityTypeName + " {");
            }

            // collection of KeyPair
            System.Collections.ArrayList keysMap = new System.Collections.ArrayList();

            for (int j = 0; j < maxNb; j++)
            {
                propName = entityType.Property[j].Name;
                propNullable = entityType.Property[j].Nullable;
                propType = getTSType(entityType.Property[j].Type);

                if (!generateJS)
                    outSLClassFile.WriteLine("  " + propName + ": " + propType + ";");

                //save key properties
                if (keysArray.Contains(propName))
                {
                    kp = new KeyPair();
                    kp.keyName = propName;
                    kp.keyType = propType;
                    keysMap.Add(kp);
                }

            }

            keysTable.Add(entityTypeName, keysMap);

            if (!generateJS)
                outSLClassFile.WriteLine("}");
           
            outSLClassFile.WriteLine("");

            // Add corresponding key entity
            if (generateJS)
            {
                writeJSClass(entityTypeName + "Key");
            }
            else
            {
                outSLClassFile.WriteLine("export class " + entityTypeName + "Key {");

                foreach (KeyPair keyPair in keysMap)
                {
                    propName = keyPair.keyName;
                    propType = getTSType(keyPair.keyType);

                    outSLClassFile.WriteLine("  " + propName + ": " + propType + ";");
                }

                outSLClassFile.WriteLine("}");
            }
            outSLClassFile.WriteLine("");

            return true;
        }

        /// <summary>
        /// Add Entity Container 
        /// </summary>
        /// <param name="entityCont"></param>
        /// <returns></returns>
        private bool addEntityContainer(EntityContainer entityCont)
        {
            string functionName;
            FunctionParams[] parameters = null;
            string paramType = null;
            string returnType = null, returnType1 = null;
            string isBindable;
            string entityName;
            string entityType;
            System.Collections.Hashtable bindableFunctions = new System.Collections.Hashtable();
            System.Collections.ArrayList functionNames = null;
            int indexSeparator = -1;
            System.Collections.Hashtable services = new System.Collections.Hashtable();
            System.Collections.ArrayList serviceFunctions = null;
            string serviceClass = null;

            try
            {
                addDefs();

                // FunctionImport oData 3.0
                // ActionImport oData 4.0
                int nbFunctionImport = entityCont.FunctionImport.Length;
                for (int j = 0; j < nbFunctionImport; j++)
                {
                    // isBindable oData 3.0
                    // isBound oData 4.0
                    isBindable = entityCont.FunctionImport[j].IsBindable;
                    functionName = entityCont.FunctionImport[j].Name;
                    parameters = null;
                    if (entityCont.FunctionImport[j].Parameter != null)
                    {
                        parameters = new FunctionParams[entityCont.FunctionImport[j].Parameter.Length];
                        for (int i = 0; i < entityCont.FunctionImport[j].Parameter.Length; i++)
                        {
                            parameters[i] = new FunctionParams();
                            parameters[i].paramName = entityCont.FunctionImport[j].Parameter[i].Name; // Name is the type of the imput parameter
                            parameters[i].paramType = entityCont.FunctionImport[j].Parameter[i].Type; // Type is the type of the object the function needs to be assigned to
                        }
                    }

                    // TO DEBUG and see values for ReturnType
                    returnType1 = entityCont.FunctionImport[j].ReturnType1;
                    returnType = null;
                    if (entityCont.FunctionImport[j].ReturnType != null)
                        returnType = entityCont.FunctionImport[j].ReturnType.Type;                    
                    if (returnType != null & returnType1 != null & returnType1 != returnType)
                         throw new Exception("FunctionImport: " + functionName + " has 2 different return types!!! (" + returnType + " and " + returnType1 + ")");                    
                    if (returnType == null)
                        returnType = returnType1;
                    
                    // Keep function associated to the type, LIMITATION: consider only 1 parameter for bindable functions
                    // we can have several bounded functions for the same type
                    // no return type
                    if ((isBindable == "true") & (parameters != null))
                    {
                        paramType = parameters[0].paramType;
                        if (bindableFunctions.ContainsKey(paramType))
                        {
                            functionNames = (System.Collections.ArrayList)bindableFunctions[paramType];
                            bindableFunctions.Remove(paramType);
                        }
                        else
                            functionNames = new System.Collections.ArrayList();

                        functionNames.Add(functionName);
                        bindableFunctions.Add(paramType, functionNames);
                    }
                    // Generate services /////                    
                    else
                    {
                        ServiceFunction service;
                        indexSeparator = functionName.IndexOf(serviceCharSeparator);
                        if (indexSeparator != -1)
                        {
                            serviceClass = functionName.Substring(0, indexSeparator);
                            service = new ServiceFunction(functionName.Substring(indexSeparator + 1), parameters, returnType);
                        }
                        //LOGIN and LOGOUT functions have no separator!!!!
                        else
                        {
                            serviceClass = "Session";
                            service = new ServiceFunction(functionName, parameters, returnType);

                            // Add class for input parameters (required for callback function)
                            if (functionName == "Login")
                            {
                                addLoginInfoClass(LoginInfoClassName, parameters);
                            }
                        }
                        
                        if (services.ContainsKey(serviceClass))
                        {
                            serviceFunctions = (System.Collections.ArrayList)services[serviceClass];
                            services.Remove(serviceClass);
                        }
                        else
                            serviceFunctions = new System.Collections.ArrayList();

                        serviceFunctions.Add(service);
                        services.Add(serviceClass, serviceFunctions);
                    }
                }
                addServices(services);
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show("Exception while adding FunctionImport" + ex.Message);
            }

            // EntitySet
            try
            {
                int nbEntitySet = entityCont.EntitySet.Length;
                for (int j = 0; j < nbEntitySet; j++)
                {
                    entityName = entityCont.EntitySet[j].Name;
                    if (entityName != "B1Sessions")
                    {
                        entityType = entityCont.EntitySet[j].EntityType;
                        ArrayList keysArray = (ArrayList)keysTable[entityType.Substring(entityType.IndexOf(".") + 1)];
                       
                        if (bindableFunctions.ContainsKey(entityType))
                            addEntitySet(entityName, entityType, keysArray, (System.Collections.ArrayList)bindableFunctions[entityType]);
                        else
                            addEntitySet(entityName, entityType, keysArray, null);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show("Exception while adding EntitySet" + ex.Message);
            }
            return true;
        }

        /// <summary>
        /// Check if if key is of type string
        /// key type needs to have '' if string
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private bool isKeyTypeString(string type)
        {
            if (type == "number")
                return false;
            
            return true;
        }

        /// <summary>
        /// Build URI for key
        /// </summary>
        /// <param name="keysArray"></param>
        /// <returns></returns>
        private string buildKeyUri(ArrayList keysArray)
        {
            string keyUri = "";
            int count = 0, nbKeys = keysArray.Count;

            foreach (KeyPair kp in keysArray)
            {
                bool isKeyString = isKeyTypeString(kp.keyType);
                keyUri += (isKeyString ? "'" : "") + "\" + " + "obj." + kp.keyName + " + \"" + (isKeyString ? "'" : "");
                count++;
                if (count < nbKeys)
                    keyUri += ", ";
            }

            return keyUri;
        }

        //JS
        //    var BusinessPartnersEntity = (function () {
        //    function BusinessPartnersEntity() {
        //    }
        //    BusinessPartnersEntity.Add = function (obj, sessionId, routeId) {
        //        callSL(B1ObjActionEnum.ADD, "BusinessPartners", sessionId, routeId, [obj]);
        //    };
        //    BusinessPartnersEntity.Update = function (obj, sessionId, routeId) {
        //        callSL(B1ObjActionEnum.UPDATE, "BusinessPartners('" + obj.CardCode + "')", sessionId, routeId, [obj]);
        //    };
        //    BusinessPartnersEntity.Delete = function (obj, sessionId, routeId) {
        //        callSL(B1ObjActionEnum.DELETE, "BusinessPartners('" + obj.CardCode + "')", sessionId, routeId, [obj]);
        //    };
        //    BusinessPartnersEntity.Get = function (obj, sessionId, routeId) {
        //        callSL(B1ObjActionEnum.GET, "BusinessPartners('" + obj.CardCode + "')", sessionId, routeId, [obj]);
        //    };
        //    return BusinessPartnersEntity;
        //}());
        //SAPB1.BusinessPartnersEntity = BusinessPartnersEntity;
        //
        // TS
        //export class BusinessPartnersEntity{
        //static Add(obj: SAPB1.BusinessPartner, sessionId: string, routeId: string) {
        //callSL(B1ObjActionEnum.ADD, "BusinessPartners", sessionId, routeId, [obj]);
        //}
        //static Update(obj: SAPB1.BusinessPartner, sessionId: string, routeId: string) {
        //callSL(B1ObjActionEnum.UPDATE, "BusinessPartners('" + obj.CardCode + "')", sessionId, routeId, [obj]);
        //}
        //static Delete(obj: SAPB1.BusinessPartnerKey , sessionId: string, routeId: string) {
        //callSL(B1ObjActionEnum.DELETE, "BusinessPartners('" + obj.CardCode + "')", sessionId, routeId, [obj]);
        //}
        //static Get(obj: SAPB1.BusinessPartnerKey , sessionId: string, routeId: string) {
        //callSL(B1ObjActionEnum.GET, "BusinessPartners('" + obj.CardCode + "')", sessionId, routeId, [obj]);
        //}
        //}
        /// <summary>
        /// Build EntitySet with all methods: Add, Update,...
        /// </summary>
        /// <param name="name"></param>
        /// <param name="type"></param>
        /// <param name="keysArray"></param>
        /// <param name="bindableFunctions"></param>
        /// <returns></returns>
        private bool addEntitySet(string name, string type, ArrayList keysArray, System.Collections.ArrayList bindableFunctions)
        {
            string uri = null;

            // Used for Get and Delete
            string typeKey = type + "Key";

            // To avoid EntitySet Name = Type
            string entityName = name + "Entity";

            if (generateJS)
            {
                outSLClassFile.WriteLine("var " + entityName + " = (function() {");
                outSLClassFile.WriteLine("function " + entityName + "() {}");
            }
            else
            {
                outSLClassFile.WriteLine("export class " + entityName + "{");
            }

            string keyUri = buildKeyUri(keysArray);

            // Add            
            uri = "\"" + name + "\"";
            writeEntityMethod(entityName, "Add", type, "ADD", uri);

            // Update
            uri = "\"" + name + "(" + keyUri + ")\"";
            writeEntityMethod(entityName, "Update", type, "UPDATE", uri);

            // Delete
            // obj not required, only key obj
            //  Need obj in order to implement the callback
            uri = "\"" + name + "(" + keyUri + ")\"";
            writeEntityMethod(entityName, "Delete", typeKey, "DELETE", uri);
 
            // Get
            // obj not required, only key obj
            //  Need obj in order to implement the callback
            uri = "\"" + name + "(" + keyUri + ")\"";
            writeEntityMethod(entityName, "Get", typeKey, "GET", uri);   

            // Bindable actions
            // LIMITATION: suppose input is key obj
            // Need obj in order to implement the callback
            if (bindableFunctions != null)
            {
                foreach (string functionName in bindableFunctions)
                {
                    uri = "\"" + name + "(" + keyUri + ")/" + functionName + "\"";
                    writeEntityMethod(entityName, functionName, type, "ACTION", uri);
                }
            }

            if (generateJS)
            {
                outSLClassFile.WriteLine("return " + entityName + ";");
                outSLClassFile.WriteLine("}());");
                outSLClassFile.WriteLine(moduleName + "." + entityName + " = " + entityName + ";");
            }
            else
            {
                outSLClassFile.WriteLine("}");
            }
            return true;
        }

        /// <summary>
        /// Build Entity method (specific Add, Update method)
        /// </summary>
        /// <param name="entityName"></param>
        /// <param name="methodName"></param>
        /// <param name="type"></param>
        /// <param name="b1Action"></param>
        /// <param name="uri"></param>
        /// <returns></returns>
        private bool writeEntityMethod(string entityName, string methodName, string type, string b1Action, string uri)
        {
            if (generateJS)
            {
                outSLClassFile.WriteLine(entityName + "." + methodName + " = function (obj, sessionId, routeId) {");
            }
            else
            {
                outSLClassFile.WriteLine("static " + methodName + "(obj: " + type + ", sessionId: string, routeId: string) {");
            }

            outSLClassFile.WriteLine("callSL(B1ObjActionEnum." + b1Action + ", " + uri + ", sessionId, routeId, [obj]);");

            if (generateJS)
                outSLClassFile.WriteLine("};");
            else
                outSLClassFile.WriteLine("}");

            return true;
        }

        /// <summary>
        /// Build Service class
        /// </summary>
        /// <param name="servicesTable"></param>
        /// <returns></returns>
        private bool addServices(System.Collections.Hashtable servicesTable)
        {
            FunctionParams[] fparams = null;
            string sparams = null, jsparams, cparams = null;
            string returnType = null;
            string uri = null;
            
            foreach (string className in servicesTable.Keys)
            {
                // create a class
                if (generateJS)
                {
                    outSLClassFile.WriteLine("var " + className + " = (function() {");
                    outSLClassFile.WriteLine("function " + className + "() {}");
                }
                else
                {
                    outSLClassFile.WriteLine("export class " + className + "{");
                }
                // add the functions inside the class
                System.Collections.ArrayList functionList = (System.Collections.ArrayList)servicesTable[className];
                foreach (ServiceFunction function in functionList)
                {
                    // format the parameters
                    sparams = "";
                    jsparams = "";
                    cparams = ""; 
                    fparams = function.parameters;
                    if (fparams != null)
                    {
                        for (int i = 0; i < fparams.Length; i++)
                        {
                            sparams += fparams[i].paramName + ": " + getTSType(fparams[i].paramType) + ",";
                            jsparams += fparams[i].paramName + ",";
                            cparams += fparams[i].paramName;
                            if (i < fparams.Length-1)
                                cparams += ",";
                        }
                    }
                    else
                        cparams = "null"; // if no parameters put null

                    // format the return type
                    returnType = ": void";
                    if (function.returnType != null)
                    {
                        returnType = ": " + getTSType(function.returnType);
                    }

                    // add the function
                    outSLClassFile.WriteLine("// Return type" + returnType);

                    // callSL inside the function
                    if (function.functionName == "Login" || function.functionName == "Logout")
                        uri = "\"" + function.functionName + "\"";
                    else 
                        uri = "\"" + className + serviceCharSeparator + function.functionName + "\"";

                    if (function.functionName == "Login")
                    {
                        sparams = "loginInfo: " + LoginInfoClassName;
                        jsparams = "loginInfo";
                        cparams = "loginInfo";
                        if (generateJS)
                        {
                            outSLClassFile.WriteLine(className + "." + function.functionName + " = function(" + jsparams + ") {");
                        }
                        else
                        {
                            outSLClassFile.WriteLine("static " + function.functionName + " (" + sparams + ") {");
                        }

                        outSLClassFile.WriteLine("callSL(B1ObjActionEnum.LOGIN, " + uri + ", \"\"" + ", \"\"" + ", [" + cparams + "]" + ");");
                    }
                    else if (function.functionName == "Logout")
                    {
                        if (generateJS)
                        {
                            outSLClassFile.WriteLine(className + "." + function.functionName + " = function(" + jsparams + " sessionId, routeId) {");
                        }
                        else
                        {
                            outSLClassFile.WriteLine("static " + function.functionName + " (" + sparams + " sessionId: string, routeId: string) {");
                        }
                        outSLClassFile.WriteLine("callSL(B1ObjActionEnum.LOGOUT, " + uri + ", sessionId, routeId" + ", [" + cparams + "]" + ");");
                    }
                    else
                    {
                        if (generateJS)
                        {
                            outSLClassFile.WriteLine(className + "." + function.functionName + " = function(" + jsparams + " sessionId, routeId) {");
                        }
                        else
                        {
                            outSLClassFile.WriteLine("static " + function.functionName + " (" + sparams + " sessionId: string, routeId: string) {");
                        }
                        outSLClassFile.WriteLine("callSL(B1ObjActionEnum.ACTION, " + uri + ", sessionId, routeId" + ", [" + cparams + "]" + ");");
                    }

                    if (generateJS)
                        outSLClassFile.WriteLine("};");
                    else
                        outSLClassFile.WriteLine("}");                    
                }

                // close the class
            if (generateJS)
            {
                outSLClassFile.WriteLine("return " + className + ";");
                outSLClassFile.WriteLine("}());");
                outSLClassFile.WriteLine(moduleName + "." + className + " = " + className  + ";");
            }
            else
            {
                outSLClassFile.WriteLine("}");
            }
            }
            return true;
        }

        /// <summary>
        /// Build LoginInfo class
        /// </summary>
        /// <param name="className"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        private bool addLoginInfoClass(string className, FunctionParams[] parameters)
        {
            if (generateJS)
                writeJSClass(className);
            else
            {
                outSLClassFile.WriteLine("export class " + className + " {");

                for (int i = 0; i < parameters.Length; i++)
                {
                    outSLClassFile.WriteLine("  " + parameters[i].paramName + ": " + getTSType(parameters[i].paramType) + ";");
                }

                outSLClassFile.WriteLine("}");
            }
            outSLClassFile.WriteLine("");

            return true;
        }

        /// <summary>
        /// Add static definitions
        /// </summary>
        /// <returns></returns>
        private bool addDefs()
        {
            if (generateJS)
            {
                outSLClassFile.WriteLine("var HttpCommandEnum = (function() {");
                outSLClassFile.WriteLine("function HttpCommandEnum() {}");
                outSLClassFile.WriteLine("HttpCommandEnum." + "POST = \"POST\";");
                outSLClassFile.WriteLine("HttpCommandEnum." + "GET = \"GET\";");
                outSLClassFile.WriteLine("HttpCommandEnum." + "DELETE = \"DELETE\";");
                outSLClassFile.WriteLine("HttpCommandEnum." + "PUT = \"PUT\";");
                outSLClassFile.WriteLine("HttpCommandEnum." + "PATCH = \"PATCH\";");
                outSLClassFile.WriteLine("return HttpCommandEnum;");
                outSLClassFile.WriteLine("}());");
                outSLClassFile.WriteLine(moduleName + ".HttpCommandEnum = HttpCommandEnum;");

                outSLClassFile.WriteLine("var B1ObjActionEnum = (function() {");
                outSLClassFile.WriteLine("function B1ObjActionEnum() {}");
                outSLClassFile.WriteLine("B1ObjActionEnum." + "LOGIN = \"Login\";");
                outSLClassFile.WriteLine("B1ObjActionEnum." + "LOGOUT = \"Logout\";");
                outSLClassFile.WriteLine("B1ObjActionEnum." + "ADD = \"Add\";");
                outSLClassFile.WriteLine("B1ObjActionEnum." + "UPDATE = \"Update\";");
                outSLClassFile.WriteLine("B1ObjActionEnum." + "GET = \"Get\";");
                outSLClassFile.WriteLine("B1ObjActionEnum." + "DELETE = \"Delete\";");
                outSLClassFile.WriteLine("B1ObjActionEnum." + "ACTION = \"Action\";");
                outSLClassFile.WriteLine("return B1ObjActionEnum;");
                outSLClassFile.WriteLine("}());");
                outSLClassFile.WriteLine(moduleName + ".B1ObjActionEnum = B1ObjActionEnum;");
            }
            else
            {
                outSLClassFile.WriteLine("export class HttpCommandEnum {");
                outSLClassFile.WriteLine("static POST = \"POST\";");
                outSLClassFile.WriteLine("static GET = \"GET\";");
                outSLClassFile.WriteLine("static DELETE = \"DELETE\";");
                outSLClassFile.WriteLine("static PUT = \"PUT\";");
                outSLClassFile.WriteLine("static PATCH = \"PATCH\";");
                outSLClassFile.WriteLine("}");

                outSLClassFile.WriteLine("export class B1ObjActionEnum {");
                outSLClassFile.WriteLine("static LOGIN = \"Login\";");
                outSLClassFile.WriteLine("static LOGOUT = \"Logout\";");
                outSLClassFile.WriteLine("static ADD = \"Add\";");
                outSLClassFile.WriteLine("static UPDATE = \"Update\";");
                outSLClassFile.WriteLine("static GET = \"Get\";");
                outSLClassFile.WriteLine("static DELETE = \"Delete\";");
                outSLClassFile.WriteLine("static ACTION = \"Action\";");
                outSLClassFile.WriteLine("}");
            }
            return true;
        }

        /// <summary>
        /// Add CallSL method (in charge of calling server side that will call SL)
        /// </summary>
        /// <returns></returns>
        private bool addCallSL()
        {
            string par = "";
            if (generateJS)
            {
                par = "b1cmd, actionUri, slsessionid, slrouteid";
            }
            else
            {
                par = "b1cmd:  SAPB1.B1ObjActionEnum, actionUri: string, slsessionid: string, slrouteid: string, ...obj: any[]";
            }
            outSLClassFile.WriteLine("function callSL(" + par + ") {");

            outSLClassFile.WriteLine("	var obj = [];");
            outSLClassFile.WriteLine("	for (var _i = 4; _i < arguments.length; _i++) {");
            outSLClassFile.WriteLine("	obj[_i - 4] = arguments[_i];");
            outSLClassFile.WriteLine("	}");

            outSLClassFile.WriteLine("	var data;");
            outSLClassFile.WriteLine("	if (b1cmd == SAPB1.B1ObjActionEnum.ADD || b1cmd == SAPB1.B1ObjActionEnum.UPDATE || b1cmd == SAPB1.B1ObjActionEnum.LOGIN)");
            outSLClassFile.WriteLine("	{");
            outSLClassFile.WriteLine("		data = JSON.stringify(obj[0][0]);");
            outSLClassFile.WriteLine("	}");

            outSLClassFile.WriteLine("		// Need to add reference to jQuery.js in html file");
            outSLClassFile.WriteLine("		$.ajax({");
            outSLClassFile.WriteLine("			type : \"POST\",");
            outSLClassFile.WriteLine("			url : \"" + B1SLProxyFileName + "?cmd=\" + b1cmd + \"&actionUri=\" + actionUri + \"&sessionID=\" + slsessionid  + \"&routeID =\" + slrouteid,");
            outSLClassFile.WriteLine("				data : data,");
            outSLClassFile.WriteLine("				dataType : \"json\",");
            outSLClassFile.WriteLine("				crossDomain : true,");
            outSLClassFile.WriteLine("				success : function(data) {");
            //outSLClassFile.WriteLine("					if (data && data.body) {");
            //outSLClassFile.WriteLine("						var msg = null;");
            //outSLClassFile.WriteLine("						msg = data.body.error.message.value;");
            //outSLClassFile.WriteLine("						$(\"#alertContainer\").html(");
            //outSLClassFile.WriteLine("								'<span id=\"error\"></span><span>' + msg + '</span>')");
            //outSLClassFile.WriteLine("								.show(\"slow\");");

            //outSLClassFile.WriteLine("						//call back");
            //outSLClassFile.WriteLine("						if (obj[0][0].onActionError)");
            //outSLClassFile.WriteLine("							obj[0][0].onActionError(data);						");
            //outSLClassFile.WriteLine("						return;");
            //outSLClassFile.WriteLine("					} else {");
            //outSLClassFile.WriteLine("						window.alert(\"Operation Successful\" + data.body);");
            //outSLClassFile.WriteLine("						$(\"#alertContainer\").html('<span id=\"error\"></span><span>' + \"Operation Successful\" + '</span>').show(\"slow\");");
            //outSLClassFile.WriteLine("");
            outSLClassFile.WriteLine("						//call back");
            outSLClassFile.WriteLine("						if (obj[0][0].onActionSuccess)");
            outSLClassFile.WriteLine("							obj[0][0].onActionSuccess(data);");
            //outSLClassFile.WriteLine("					}");
            outSLClassFile.WriteLine("				},");
            outSLClassFile.WriteLine("				error : function(response, textStatus, errorThrown) {");
            //outSLClassFile.WriteLine("					var status = response.status;");
            //outSLClassFile.WriteLine("					var msg = response.responseText;");
            //outSLClassFile.WriteLine("");
            //outSLClassFile.WriteLine("					$(\"#alertContainer\").html('<span id=\"error\"></span><span>' + msg + '</span>').show(\"slow\");");

            outSLClassFile.WriteLine("					//call back");
            outSLClassFile.WriteLine("					if (obj[0][0].onActionError)");
            outSLClassFile.WriteLine("						obj[0][0].onActionError(response);");
            outSLClassFile.WriteLine("				}");
            outSLClassFile.WriteLine("		});");
            outSLClassFile.WriteLine("}");
            outSLClassFile.WriteLine("");

            return true;
        }

        /// <summary>
        /// Build a simple JS class
        /// </summary>
        /// <param name="className"></param>
        private void writeJSClass(string className)
        {
            outSLClassFile.WriteLine("var " + className + " = (function() {");
            outSLClassFile.WriteLine("function " + className + "() {}");
            outSLClassFile.WriteLine("return " + className + ";");
            outSLClassFile.WriteLine("}());");
            outSLClassFile.WriteLine(moduleName + "." + className + " = " + className + ";");
        }

        /// <summary>
        /// Get TypeScript type associated to an Edm type
        /// </summary>
        /// <param name="oDataType"></param>
        /// <returns></returns>
        private string getTSType(string oDataType)
        {
            string tsType = oDataType;

            if (oDataType == "Edm.String")
                tsType = "string";
            else if (oDataType == "Edm.Int32")
                tsType = "number";
            else if (oDataType == "Edm.Double")
                tsType = "number";
            else if (oDataType == "Edm.DateTime")
                tsType = "Date";
            else if (oDataType == "Edm.Time") /*Need to find out if type Date is correct or not*/
                tsType = "Date";
            else if (oDataType.StartsWith("Collection("))
                tsType = oDataType.Substring(oDataType.IndexOf("(") + 1, oDataType.Length - (oDataType.IndexOf("(") + 1) - 1) + "[]";

            return tsType;
}
    }
}
