import React, { useEffect, useRef } from 'react';

export default function UseEffectIf<T = any>(callBack: () => void, values: T[], initState: T[], compareCallBack: (prevValues: T[]) => boolean) {
    const prevState = useRef(initState);
    useEffect(() => {
        if (compareCallBack(prevState.current)) {
            callBack()
        }
        prevState.current = values;
    }, values)
}