import type {Metadata} from 'next';
import './globals.css';
import Providers from '@/components/Providers';
import {ReactNode} from 'react';
import TopNav from '@/components/nav/TopNav';
import SideMenu from '@/components/SideMenu';
import ClientOnly from '@/app/ClientOnly';

export const metadata: Metadata = {
    title: {
        template: '%s | Overflow',
        default: 'Overflow'
    },
    description: 'The Questions App for PowerNerds'
};

export default function RootLayout({children}: Readonly<{ children: ReactNode }>) {
    return (
        <html lang='en' className='h-full'>
        <body className='flex flex-col bg-stone-200 dark:bg-default-50 h-full'>
        <ClientOnly>
            <Providers>
                <TopNav/>
                <div className='flex grow overflow-auto'>
                    <aside
                        className='basis-1/6 shrink-0 shadow-sm pt-20 sticky top-0 px-6 bg-white dark:bg-default-50 
                        dark:border-r dark:border-neutral-500'
                    >
                        <SideMenu/>
                    </aside>
                    <main className='flex-1 pt-20 h-full dark:border-r dark:border-neutral-500'>
                        {children}
                    </main>
                    <aside className='basis-1/4 shrink-0 px-6 pt-20 bg-white dark:bg-default-50 sticky top-0'>
                        Right content
                    </aside>
                </div>
            </Providers>
        </ClientOnly>
        </body>
        </html>
    );
}
