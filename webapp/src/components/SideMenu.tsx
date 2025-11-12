'use client'

import {Listbox, ListboxItem} from '@heroui/listbox';
import {HomeIcon, QuestionMarkCircleIcon, TagIcon, UsersIcon} from '@heroicons/react/24/outline';
import {usePathname} from 'next/navigation';

export default function SideMenu() {
    const pathname = usePathname();
    
    const navLinks = [
        {key: 'home', icon: HomeIcon, text: 'Home', href: '/'},
        {key: 'tags', icon: TagIcon, text: 'Tags', href: '/tags'},
        {key: 'questions', icon: QuestionMarkCircleIcon, text: 'Questions', href: '/questions'},
        {key: 'session', icon: UsersIcon, text: 'User Session', href: '/session'}
    ]
    return (
        <Listbox aria-label='nav-links' variant='faded' items={navLinks} className='sticky top-20 ml-6'>
            {({key, href, icon: Icon, text}) => (
                <ListboxItem
                    href={href}
                    aria-labelledby={key}
                    aria-describedby={text}
                    key={key}
                    startContent={<Icon className='h-6'/>}
                    classNames={{
                        base: pathname === href ? 'text-secondary' : '',
                        title: 'text-lg'
                    }}
                >
                    {text}
                </ListboxItem>
            )}
        </Listbox>
    );
}