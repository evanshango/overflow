'use client'

import {User} from 'next-auth';
import {Dropdown, DropdownItem, DropdownMenu, DropdownTrigger} from '@heroui/dropdown';
import {Avatar} from '@heroui/avatar';
import {signOut} from 'next-auth/react';

type Props = {
    user: User
}
export default function UserMenu({user}: Props) {
    return (
        <Dropdown>
            <DropdownTrigger>
                <div className='flex items-center gap-2 cursor-pointer'>
                    <Avatar color='secondary' size='md' name={user.name?.charAt(0).toUpperCase()} className='rounded-xl' />
                    <div className='flex flex-col items-start'>
                        <span className='font-bold text-sm'>{user.name}</span>
                        <span className='font-extralight text-xs'>{user.email}</span>
                    </div>
                </div>
            </DropdownTrigger>
            <DropdownMenu>
                <DropdownItem key='edit'>Edit Profile</DropdownItem>
                <DropdownItem
                    key='signout'
                    className='text-danger'
                    color='danger'
                    onClick={() => signOut({redirectTo: '/'})}
                >
                    Sign out
                </DropdownItem>
            </DropdownMenu>
        </Dropdown>
    );
}