export function GetDateStringFromDateTime(date:Date){
    return date.toISOString().slice(0,10);
}