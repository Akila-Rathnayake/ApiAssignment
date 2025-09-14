
## **Prerequisites**

- .NET installed => https://dotnet.microsoft.com/download
- VS Code IDE
- Internet connection to access the API: `https://api.restful-api.dev`
- Recommended VS Code extensions:
  - C# Dev Kit (by Microsoft) - Install this under vs code extensions 

---

## **Project Structure**

- `ApiAssignment.csproj` → Project file defining the .NET target framework and all dependencies for the test project.
- `RestfulApiCrudTests.cs` → Main test file containing all CRUD tests.
- `PriorityOrderer.cs` → Custom xUnit test orderer to run tests in a specific sequence.
- `README.md` → This file.

---

## **Test Details**

The tests are executed in the following order using **TestPriority**:

1. **Get_All_Objects_ShouldReturnList** → Retrieves all objects and validates at least 13 exist.
2. **Create_Object_ShouldReturnId** → Adds a new object and validates returned fields.
3. **Get_Object_By_Id_ShouldReturnCorrectObject** → Fetches the newly created object by ID.
4. **Update_Object_ShouldChangeValues** → Updates the object and validates updated fields.
5. **Delete_Object_ShouldReturnMessage** → Deletes the object and confirms deletion.

The `_createdObjectId` variable is used to share the object ID between tests.

---
## **How to Run Tests in VS Code**

### **Step 1: Clone the project**
Clone the project 
git clone https://github.com/Akila-Rathnayake/ApiAssignment.git

### **Step 2: Build the Project**

Open the project folder in VS Code, then open the terminal (`Ctrl + ~`) and run: 
dotnet build

### **Step 3: Run the Project**
After the build is successful, run all tests: 
dotnet test
