import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Trains } from './trains';

describe('Trains', () => {
  let component: Trains;
  let fixture: ComponentFixture<Trains>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Trains],
    }).compileComponents();

    fixture = TestBed.createComponent(Trains);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
