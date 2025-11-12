'use client'

import {Button} from '@heroui/button';
import Link from 'next/link';

export default function NotFound() {
    return (
        <div className='h-full flex items-center justify-center'>
            <div className='text-center space-y-6'>
                <h1 className='text-5xl font-bold'>404 - Page Not Found</h1>
                <p className='text-lg text-base-content/80'>
                    Sorry, the page you are looking for does not exist.
                </p>  
                <Button as={Link} href='/' color='primary' className='rounded-full bg-secondary px-5'>
                    Go Home
                </Button>
            </div>
        </div>
    );
}