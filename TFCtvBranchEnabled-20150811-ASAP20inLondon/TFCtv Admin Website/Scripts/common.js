/* File Created: July 30, 2012 */
function disableButton(button, value) {
    if (value == '' || value == undefined)
        button.val('Please wait...');
    else
        button.val(value);
    button.attr('disabled', 'disabled');
}

function enableButton(button, value) {
    button.removeAttr('disabled');
    if (value == '' || value == undefined)
        button.val('Submit');
    else
        button.val(value);
}

function hideElement(e, empty) {
    if (empty) {
        e.empty();
    }    
    e.hide();
 }