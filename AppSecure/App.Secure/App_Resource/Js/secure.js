
var nextRequestId = 0;
var jsonSecureHandler = "/Handler/JsonSecure.ashx";
var jsonAuthSecureHandler = "/Handler/JsonAuthSecure.ashx";

/*****************************************************************************************************************************
                                                       Action Methods
******************************************************************************************************************************/

function fillRolesSelectTag(roleSelectElmId, msgElmId, selectIndex) {
    loadSelectOption(jsonAuthSecureHandler, "GetRoleOptionList", roleSelectElmId, msgElmId, selectIndex);
}

function fillRoleUsersUvTag(placeHolderElmId, msgElmId, pageNo, itemsPerPage) {
    loadULHtml(jsonAuthSecureHandler, "GetRoleUserListHtml", placeHolderElmId, msgElmId, pageNo, itemsPerPage);
}

/*****************************************************************************************************************************
                                                       WebService Methods (Secure Handler)
******************************************************************************************************************************/

//LogOff User
function logOffUser(msgElmId) {
    var valid = true;

    //Validate Arguments

    if (valid == true) {
        var paramValues = '{';
        paramValues += '}';

        var request = {
            id: ++nextRequestId,
            method: "LogOffUser",
            params: paramValues
        };

        asyncJsonRequest(jsonSecureHandler, request, msgElmId,
           function (response) {
               var haveResult = false;
               if (response.hasOwnProperty('Result') === true) {
                   var retResult = response.Result;
                   if (retResult.hasOwnProperty('message') === true) {
                       window.location.replace('/Index.ashx');
                       var retMessage = retResult.message;
                       showSuccess(retMessage, msgElmId);
                   }
                   else if (retResult.hasOwnProperty('error') === true) {

                       var retError = retResult.error;
                       showError(retError, msgElmId);
                   }
               }
           });
    }
}

//Login User
function loginUser(userName, userPassword, rememberMe, msgElmId) {
    var valid = true;

    //Validate Arguments
	
    if (valid == true) {
        var paramValues = '{';
        paramValues += '"userName" : "' + encodeURIComponent(userName) + '"';
        paramValues += ', "userPassword" : "' + encodeURIComponent(userPassword) + '"';
        paramValues += ', "rememberMe" : ' + rememberMe;
        paramValues += '}';

        var request = {
            id: ++nextRequestId,
            method: "LoginUser",
            params: paramValues
        };

        asyncJsonRequest(jsonSecureHandler, request, msgElmId,
           function (response) {
               var haveResult = false;
               if (response.hasOwnProperty('Result') === true) {
                   var retResult = response.Result;
                   if (retResult.hasOwnProperty('message') === true) {
                       window.location.replace('/Index.ashx');
                       var retMessage = retResult.message;
                       showSuccess(retMessage, msgElmId);
                   }
                   else if (retResult.hasOwnProperty('error') === true) {

                       var retError = retResult.error;
                       showError(retError, msgElmId);
                   }
               }
           });
    }
}

//Register User
function registerUser(userId, userEmail, userPassword, msgElmId) {
    var valid = true;

    //Validate Arguments
	
    if (valid == true) {
        var paramValues = '{';
        paramValues += ', "userId" : "' + encodeURIComponent(userId) + '"';
        paramValues += ', "userEmail" : "' + encodeURIComponent(userEmail) + '"';
        paramValues += ', "userPassword" : "' + encodeURIComponent(userPassword) + '"';
        paramValues += '}';

        var request = {
            id: ++nextRequestId,
            method: "RegisterUser",
            params: paramValues
        };

        asyncJsonRequest(jsonAuthSecureHandler, request, msgElmId,
           function (response) {
               var haveResult = false;
               if (response.hasOwnProperty('Result') === true) {
                   var retResult = response.Result;
                   if (retResult.hasOwnProperty('message') === true) {
                       var retMessage = retResult.message;
                       showSuccess(retMessage, msgElmId);
                   }
                   else if (retResult.hasOwnProperty('error') === true) {

                       var retError = retResult.error;
                       showError(retError, msgElmId);
                   }
               }
           });
    }
}

//Add User to Role
function addUserToRole(userId, userRole, msgElmId) {
    var valid = true;

    //Validate Arguments
	
    if (valid == true) {
        var paramValues = '{';
        paramValues += ', "userId" : "' + encodeURIComponent(userId) + '"';
        paramValues += ', "roleName" : "' + encodeURIComponent(userRole) + '"';
        paramValues += '}';

        var request = {
            id: ++nextRequestId,
            method: "AddUserToRole",
            params: paramValues
        };

        asyncJsonRequest(jsonAuthSecureHandler, request, msgElmId,
           function (response) {
               var haveResult = false;
               if (response.hasOwnProperty('Result') === true) {
                   var retResult = response.Result;
                   if (retResult.hasOwnProperty('message') === true) {
                       var retMessage = retResult.message;
                       showSuccess(retMessage, msgElmId);
                       if (typeof(refreshRoleUsersUvTag) === "function")
                       {
                            refreshRoleUsersUvTag();
                       }
                   }
                   else if (retResult.hasOwnProperty('error') === true) {

                       var retError = retResult.error;
                       showError(retError, msgElmId);
                   }
               }
           });
    }
}

//Remove User to Role
function removeUserFromRole(userId, userRole, msgElmId) {
    var valid = true;

    //Validate Arguments
	
    if (valid == true) {
        var paramValues = '{';
        paramValues += ', "userId" : "' + encodeURIComponent(userId) + '"';
        paramValues += ', "roleName" : "' + encodeURIComponent(userRole) + '"';
        paramValues += '}';

        var request = {
            id: ++nextRequestId,
            method: "RemoveUserFromRole",
            params: paramValues
        };

        asyncJsonRequest(jsonAuthSecureHandler, request, msgElmId,
           function (response) {
               var haveResult = false;
               if (response.hasOwnProperty('Result') === true) {
                   var retResult = response.Result;
                   if (retResult.hasOwnProperty('message') === true) {
                       var retMessage = retResult.message;
                       showSuccess(retMessage, msgElmId);
                       if (typeof(refreshRoleUsersUvTag) === "function")
                       {
                            refreshRoleUsersUvTag();
                       }
                   }
                   else if (retResult.hasOwnProperty('error') === true) {

                       var retError = retResult.error;
                       showError(retError, msgElmId);
                   }
               }
           });
    }
}
/*****************************************************************************************************************************
                                                       Action Methods (LogOff, Login, Register)
******************************************************************************************************************************/

function btnLogOffUser(msgElmId) {

    var msgElm = document.getElementById(msgElmId);

    if (msgElm) {

        msgElm.innerHTML = "";
        msgElm.className = "";

        logOffUser(msgElmId);
    }
}

function btnLoginUser(userNameElmId, userPasswordElmId, rememberElmId, msgElmId) {
	
    var msgElm = document.getElementById(msgElmId);
	
    var userNameElm = document.getElementById(userNameElmId);
    var userPasswordElm = document.getElementById(userPasswordElmId);
    var rememberElm = document.getElementById(rememberElmId);

    if ((userNameElm) && (userPasswordElm) && (rememberElm) 
        && (msgElm)) 
        {
        
        msgElm.innerHTML = "";
        msgElm.className = "";
        
        if (userNameElm.value.length == 0) {
            msgElm.className = "error";
            msgElm.innerHTML = "Please enter User Name";
            $(msgElm).show();
        }
        else if (userPasswordElm.value.length == 0) {
            msgElm.className = "error";
            msgElm.innerHTML = "Please enter User Password";
            $(msgElm).show();
        }
        else {
            if (rememberElm.checked == true)
            {
                loginUser(userNameElm.value, userPasswordElm.value, true, msgElmId);
            }
            else
            {
                loginUser(userNameElm.value, userPasswordElm.value, false, msgElmId);
            }            
        }
    }
}

function btnRegisterUser(userIdElmId, userEmailElmId, userPasswordElmId, userPasswordConfirmElmId, msgElmId) {
	
    var msgElm = document.getElementById(msgElmId);
    msgElm.innerHTML = "";
    msgElm.className = "";
	
    var userIdElm = document.getElementById(userIdElmId);
    var userEmailElm = document.getElementById(userEmailElmId);
    var userPasswordElm = document.getElementById(userPasswordElmId);
    var userPasswordConfirmElm = document.getElementById(userPasswordConfirmElmId);

    if ((userIdElm) && (userEmailElm)  
        && (userPasswordElm) && (userPasswordConfirmElm)
        && (msgElm)) 
        {
        
        if (userIdElm.value.length == 0) {
            msgElm.className = "error";
            msgElm.innerHTML = "Please enter User Id";
            $(msgElm).show();
        }
        else if (userEmailElm.value.length == 0) {
            msgElm.className = "error";
            msgElm.innerHTML = "Please enter User Email";
            $(msgElm).show();
        }
        else if (userPasswordElm.value.length == 0) {
            msgElm.className = "error";
            msgElm.innerHTML = "Please enter User Password";
            $(msgElm).show();
        } 
        else if (userPasswordConfirmElm.value.length == 0) {
            msgElm.className = "error";
            msgElm.innerHTML = "Please enter User Confirm Password";
            $(msgElm).show();
        } 
        else if (userPasswordElm.value != userPasswordConfirmElm.value) {
            msgElm.className = "error";
            msgElm.innerHTML = "Please Enter the Password and Confirm Password as the Same";
            $(msgElm).show();
        } 
        else {
            //Register User
            registerUser(userIdElm.value, userEmailElm.value, userPasswordElm.value, msgElmId);
        }
    }
}

function btnAddRemoveUserRole(addUser, userIdElmId, roleElmId, msgElmId) {
	
    var msgElm = document.getElementById(msgElmId);
    msgElm.innerHTML = "";
    msgElm.className = "";
	
    var userIdElm = document.getElementById(userIdElmId);
    var roleElm = document.getElementById(roleElmId);

    if ((userIdElm) && (roleElm)  
        && (msgElm)) 
        {
        
        if (userIdElm.value.length == 0) {
            msgElm.className = "error";
            msgElm.innerHTML = "Please enter User Id";
            $(msgElm).show();
        }
        else if (roleElm.value.length == 0) {
            msgElm.className = "error";
            msgElm.innerHTML = "Please select User Role";
            $(msgElm).show();
        }
        else {
            if (addUser === true)
            {
                //Add Role
                addUserToRole(userIdElm.value, roleElm.value, msgElmId);
            }else
            {
                //Remove Role
                removeUserFromRole(userIdElm.value, roleElm.value, msgElmId);
            }            
        }
    }
}

