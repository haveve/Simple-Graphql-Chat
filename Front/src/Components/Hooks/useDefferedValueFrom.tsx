import React, { useEffect, useRef, useState } from 'react';
import { Subscription, interval } from 'rxjs';

export default function useDefferedValueFrom<T>(currentValue: T, fromSeconds: number) {

    const sub = useRef<Subscription | null>(null);
    const [current, setCurrent] = useState<T | null>(null);
    const inCache = useRef<T | null>(null);

    useEffect(() => {
        if (!current) {
            setCurrent(currentValue);
            inCache.current = currentValue;
            return;
        }
        sub.current?.unsubscribe();
        inCache.current = currentValue;

        sub.current = interval(fromSeconds * 1000).subscribe(() => {
            setCurrent(inCache.current);
        });

    }, [currentValue])

    return current;
}