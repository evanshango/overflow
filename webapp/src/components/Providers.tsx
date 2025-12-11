'use client'

import {HeroUIProvider, ToastProvider} from '@heroui/react';
import {ReactNode, useEffect} from 'react';
import {useRouter} from 'next/navigation';
import {ThemeProvider as NextThemesProvider} from 'next-themes';
import {getTags} from '@/lib/actions/tag-actions';
import {useTagStore} from '@/lib/hooks/useTagStore';

export default function Providers({children}: { children: ReactNode }) {
    const router = useRouter();
    const setTags = useTagStore(state => state.setTags);

    useEffect(() => {
        const loadTags = async () => {
            const {data: tags} = await getTags();
            if (tags) setTags(tags);
        }
        void loadTags();
    }, [setTags]);

    return (
        <HeroUIProvider navigate={router.push} className='h-full flex flex-col'>
            <ToastProvider/>
            <NextThemesProvider attribute='class' defaultTheme='system'>
                {children}
            </NextThemesProvider>
        </HeroUIProvider>
    );
}