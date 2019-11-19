import { MatChipInputEvent } from '@angular/material';

interface IAddStr {
    searchString: string[];
    selectedString: string;
}

export const SearchString = {
    addStr: (
        event: MatChipInputEvent,
        searchString: string[],
        selectedString: string,
        callback: (searchString: string[], selectedString: string) => void,
    ): IAddStr => {
        const value = (event.value || '').trim().toUpperCase();

        if (!value || value === selectedString || searchString.indexOf(value) >= 0) {
            callback(searchString, '');
            return;
        }

        const newSearchString = searchString.slice();
        let newSelectedString = `${selectedString}`;
        const inputKeyValue = value.split(':');

        // get index of the selected string in searchString array
        const selectedIndex = newSearchString.indexOf(newSelectedString);

        if (inputKeyValue[0]) {
            const valueIsAttr = value.indexOf(':') >= 0;
            const sameKeyIndex = newSearchString.findIndex((str) => {
                const strIsAttr = str.indexOf(':') >= 0;

                // if the input value is attribute search and the search string is not, they are different
                // if the input value is simple text search and the search string is not, they are different
                if ((valueIsAttr && !strIsAttr) || (!valueIsAttr && strIsAttr)) {
                    return false;
                }

                const keyValue = str.split(':');
                return keyValue[0] && keyValue[0] === inputKeyValue[0];
            });

            if (sameKeyIndex >= 0) {
                const inputValues = inputKeyValue[1].split('||');
                let newValues;

                // check if selected search string is the same search string
                const isSameIndex = selectedIndex === sameKeyIndex;

                if (isSameIndex) {
                    newValues = inputValues.slice();
                } else {
                    // get key/value of the selected search string
                    const sameKeyValue = newSearchString[sameKeyIndex].split(':');

                    // split values of the selected search string
                    const sameKeyValues = sameKeyValue[1].split('||');

                    // generate new values by concat selected search string values and input values
                    newValues = inputValues.concat(sameKeyValues);
                }

                // unify new values
                newValues = newValues.filter((val, index) => newValues.indexOf(val) === index);

                // create new value string
                const newValue = `${inputKeyValue[0]}:` + newValues.join('||');

                if (newSelectedString) {
                    if (isSameIndex) {
                        newSearchString[selectedIndex] = newValue;
                    } else {
                        newSearchString[sameKeyIndex] = newValue;
                        newSearchString.splice(selectedIndex, 1);
                    }
                } else {
                    newSearchString[sameKeyIndex] = newValue;
                }
            } else {
                if (newSelectedString) {
                    newSearchString[selectedIndex] = value;
                } else if (newSearchString.indexOf(value) === -1) {
                    newSearchString.push(value);
                }
            }

            if (newSelectedString) {
                newSelectedString = '';
            }
        } else {
            if (newSelectedString) {
                newSearchString[selectedIndex] = value;
            } else if (newSearchString.indexOf(value) === -1) {
                newSearchString.push(value);
            }
        }

        callback(newSearchString, newSelectedString);
    },
};
