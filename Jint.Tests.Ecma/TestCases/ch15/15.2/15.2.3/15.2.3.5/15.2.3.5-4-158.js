/// Copyright (c) 2012 Ecma International.  All rights reserved. 
/**
 * @path ch15/15.2/15.2.3/15.2.3.5/15.2.3.5-4-158.js
 * @description Object.create - 'value' property of one property in 'Properties' is own accessor property (8.10.5 step 5.a)
 */


function testcase() {

        var descObj = {};

        Object.defineProperty(descObj, "value", {
            get: function () {
                return "ownAccessorProperty";
            }
        });

        var newObj = Object.create({}, {
            prop: descObj
        });

        return newObj.prop === "ownAccessorProperty";
    }
runTestCase(testcase);
