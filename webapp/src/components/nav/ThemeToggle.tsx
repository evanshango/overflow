'use client'

import {Button} from '@heroui/button';
import {useTheme} from 'next-themes';
import {MoonIcon, SunIcon} from '@heroicons/react/24/solid';

export default function ThemeToggle() {
    const {theme, setTheme} = useTheme();

    return (
        <Button
            color='primary'
            variant='light'
            isIconOnly
            aria-label='Toggle Theme'
            onPress={() => setTheme(theme === 'light' ? 'dark' : 'light')}
            className='rounded-full'
        >
            {theme === 'light' ? (
                <MoonIcon className='h-8'/>
            ) : (
                <SunIcon className='h-8 text-yellow-300'/>
            )}
        </Button>
    );
}