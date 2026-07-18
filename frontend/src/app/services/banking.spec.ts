import { TestBed } from '@angular/core/testing';

import { Banking } from './banking';

describe('Banking', () => {
  let service: Banking;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(Banking);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
