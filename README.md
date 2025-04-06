# MembershipRegistrationApp_MongoDb
In order to buil and run the application:
1. Install MongoDB server:
- Go to https://www.mongodb.com/try/download/community.

2. Using the database:
- Download the exported MongoDB data from file: jasenet.json.
- To import the MongoDB data from the jasenet.json file, use the following command in your terminal: mongoimport --uri="mongodb://localhost:27017" --collection=jasenet --file=jasenet.json --jsonArray

3. Run the program:
- Open the project in Visual Studio.

4. The functionality of the application:
- Creating, updating, deleting, and searching for members in the application.
