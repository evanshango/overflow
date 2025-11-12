'use client'

import {ReactNode, useEffect, useState} from 'react';

export default function ClientOnly({children}: { children: ReactNode }) {
    const [hasMounted, setHasMounted] = useState(false);
    useEffect(() => {
        const id = requestAnimationFrame(() => setHasMounted(true));
        return () => cancelAnimationFrame(id);
    }, []);

    if (!hasMounted) {
        return null;
    }

    return <>{children}</>;
}